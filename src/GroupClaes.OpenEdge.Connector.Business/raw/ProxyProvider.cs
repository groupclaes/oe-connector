using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Progress.Open4GL.Proxy;
using System;
using System.Collections.Concurrent;
using System.Threading;

using GroupClaes.OpenEdge.Connector.Business.Raw.Internal;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal class ProxyProvider : IProxyProvider
  {
    private readonly ConcurrentDictionary<string, ProxyConnection> activeConnections;
    private readonly ILogger<ProxyProvider> logger;
    private readonly IConfigurationSection configurationSection;
    private readonly IChecksumService checksumService;
    private readonly IServiceProvider serviceProvider;
    public ProxyProvider(ILogger<ProxyProvider> logger, IConfiguration configuration,
      IServiceProvider serviceProvider, IChecksumService checksumService)
    {
      this.logger = logger;

      configurationSection = configuration.GetSection("OpenEdge");
      this.serviceProvider = serviceProvider;
      this.checksumService = checksumService;
      activeConnections = new ConcurrentDictionary<string, ProxyConnection>();
    }

    public bool CloseConnection(ProxyConnection connection)
    {
      if (connection != null)
      {
        foreach(ProxyInterface nestedInterface in connection.ProxyInterfaces)
        {
          nestedInterface.CancelAllRequests();
        }

        connection.Connection.ReleaseConnection();
        connection.Connection.Dispose();
        connection.ProxyInterfaces.Clear();

        return activeConnections.TryRemove(connection.Hash, out _);
      }

      return false;
    }
    public bool CloseConnection(IProxyInterface proxyInterface)
    {
      if (proxyInterface is ProxyInterface proxy)
      {
        return CloseConnection(proxy.connection);
      }

      return false;
    }

    public IProxyInterface CreateProxyInstance()
      => CreateProxyInstance(Constants.DefaultOpenEdgeEndpoint, null, null, null);
    public IProxyInterface CreateProxyInstance(string appServer)
      => CreateProxyInstance(appServer, null, null, null);
    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password)
      => CreateProxyInstance(appServer, userId, password, null);
    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password,
      string appServerInfo)
      => CreateProxyInstance(appServer, userId, password, appServerInfo, null);

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo, string procedurePrefix)
    {
      ProcedureCredentials credentials = new ProcedureCredentials
      {
        AppServer = appServer,
        Username = userId,
        Password = password,
        AppId = appServerInfo,
        ProcedurePrefix = procedurePrefix
      };

      return this.CreateProxyInstance(credentials);
    }
    public IProxyInterface CreateProxyInstance(ProcedureCredentials credentials)
    {
      ProxyConnection connection = GetCachedConnection(credentials);
      #if !DEBUG
            credentials.Password = null;
      #endif
      logger.LogDebug("Retrieved app server config for {Endpoint} with credentials {@Credentials}", credentials.Endpoint, credentials);

      try
      {
        if (!string.IsNullOrWhiteSpace(credentials.ProcedurePrefix))
        {
          return new PrefixedProxyInterface(GetLogger<PrefixedProxyInterface>(), connection,
            credentials.ProcedurePrefix);
        }
        else
        {
          return new ProxyInterface(GetLogger<ProxyInterface>(), connection);
        }
      }
      catch (Exception)
      {
        this.CloseConnection(connection);
        throw;
      }
    }

#region private
    private AppServerConfig GetAppServerConfig(string appServer)
    {
      AppServerConfig appConfig = new AppServerConfig();
      configurationSection.GetSection("Appservers")
        .Bind(appServer, appConfig);

      return appConfig;
    }

    private ILogger<T> GetLogger<T>() where T : IProxyInterface
      => serviceProvider.GetRequiredService<ILogger<T>>();

    private void SetTraceLogger()
    {
      var traceLogger = serviceProvider.GetRequiredService<TracerLogger>();
      Progress.Open4GL.RunTimeProperties.tracer.startTrace(traceLogger, 0, "O4GL ", "Trace");
    }

    private ProxyConnection GetCachedConnection(ProcedureCredentials credentials)
    {
      AppServerConfig config = GetAppServerConfig(credentials.AppServer);
      ValidateCredentials(credentials, config);

      string credentialsHash = GenerateCredentialsHash(credentials);
      SetTraceLogger();

      Func<string, ProxyConnection> createConnection = (hash) =>
      {
        logger.LogDebug("Generating new connection for {Endpoint} with config {Hash}", config.Endpoint, hash);
        Connection connection = new Connection(
          credentials.Endpoint,
          credentials.Username,
          credentials.Password,
          credentials.AppId);

        connection.WaitIfBusy = true;

        return new ProxyConnection(credentialsHash, connection);
      };

      ProxyConnection connection = activeConnections.GetOrAdd(credentialsHash, createConnection);
#if DEBUG
      logger.LogTrace("Connection: {HashCode}", connection.GetHashCode());
#endif

      return connection;
    }
    /// <summary>
    /// Validate the provided procedure credentials
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    private ProcedureCredentials ValidateCredentials(ProcedureCredentials credentials, AppServerConfig config)
    {
      if (string.IsNullOrWhiteSpace(credentials.ProcedurePrefix) && config.PathPrefix != null)
      {
        credentials.ProcedurePrefix = config.PathPrefix;
      }

      if (string.IsNullOrWhiteSpace(credentials.Endpoint))
      {
        credentials.Endpoint = config.Endpoint;
      }

      if (string.IsNullOrWhiteSpace(credentials.AppId))
      {
        credentials.AppId = string.IsNullOrEmpty(config.AppId) ?
          "default" : config.AppId;
      }

      if (string.IsNullOrWhiteSpace(credentials.Username)
        || string.IsNullOrWhiteSpace(credentials.Password))
      {
        credentials.Username = config.Username;
        credentials.Password = config.Password;
      }

      return credentials;
    }
    /// <summary>
    /// Generate a hash based on the provided credentials
    /// </summary>
    /// <param name="credentials"></param>
    /// <returns></returns>
    private string GenerateCredentialsHash(ProcedureCredentials credentials)
      => checksumService.Generate(string.Format("${0}${1}${2}${3}$",
        credentials.AppServer,
        credentials.Endpoint,
        credentials.Username,
        credentials.Password));
  }
#endregion
}
