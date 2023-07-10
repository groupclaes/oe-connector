using GroupClaes.OpenEdge.Connector.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Progress.Open4GL.Proxy;
using System;
using System.Threading;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal class ProxyProvider : IProxyProvider
  {
    private const int MaxConnections = 10;
    private const int MaxFailedAttempts = 5;
    internal static int ActiveProviders { get; private set; }
    internal static int FailedCount { get; private set; }
    private static SemaphoreSlim providerLock = new SemaphoreSlim(1);

    private readonly ILogger<ProxyProvider> logger;

    private readonly IConfigurationSection configurationSection;
    private readonly IServiceProvider serviceProvider;
    public ProxyProvider(ILogger<ProxyProvider> logger, IConfiguration configuration, IServiceProvider serviceProvider)
    {
      this.logger = logger;

      configurationSection = configuration.GetSection("OpenEdge");
      this.serviceProvider = serviceProvider;
    }

    public IProxyInterface CreateProxyInstance()
      => CreateProxyInstance(Constants.DefaultOpenEdgeEndpoint, null, null, null);

    public IProxyInterface CreateProxyInstance(string appServer)
      => CreateProxyInstance(appServer, null, null, null);

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password)
      => CreateProxyInstance(appServer, userId, password, null);

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password,
      string appServerInfo)
    {

      SetTraceLogger();

      if (ActiveProviders < MaxConnections)
      {
        ResetFailedCount();
        AppServerConfig config = GetAppServerConfig(appServer);

        Connection connection = new Connection(config.Endpoint,
          userId ?? config.Username,
          password ?? config.Password,
          appServerInfo ?? config.AppId);
        config.Password = null;

        logger.LogDebug("Retrieved app server config for {Endpoint} with config {@Config}", config.Endpoint, config);

        AddActiveProvider();
        IProxyInterface proxyInterface = new ProxyInterface(GetLogger<ProxyInterface>(), connection);
        return proxyInterface;
      }
      else
      {
        AddFailedCount();
        throw new Exception("Active providers overreached");
      }
    }

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo, string procedurePrefix)
    {
      SetTraceLogger();
      if (ActiveProviders < MaxConnections)
      {
        ResetFailedCount();
        AppServerConfig config = GetAppServerConfig(appServer);
        Connection connection = new Connection(config.Endpoint,
          userId ?? config.Username,
          password ?? config.Password,
          appServerInfo ?? config.AppId);
        config.Password = null;

        logger.LogDebug("Retrieved app server config for {Endpoint} with config {@Config}", config.Endpoint, config);

        AddActiveProvider();
        IProxyInterface proxyInterface = new PrefixedProxyInterface(GetLogger<PrefixedProxyInterface>(),
          connection, procedurePrefix ?? config.PathPrefix);

        return proxyInterface;
      }
      else
      {
        AddFailedCount();
        throw new Exception("Active providers overreached");
      }
    }

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

    private void AddActiveProvider()
    {
      providerLock.Wait();
  
      ActiveProviders++;
      logger.LogCritical("Creating ProxyInterface, Active Providers {ActiveProviders}", ProxyProvider.ActiveProviders);

      providerLock.Release();
    }

    public static void RemoveActiveProvider()
    {
      providerLock.Wait();
      ActiveProviders--;
      
      providerLock.Release();
    }

    private static void AddFailedCount()
    {
      providerLock.Wait();
      FailedCount++;
      providerLock.Release();

      if (FailedCount > MaxFailedAttempts)
      {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
      }
    }

    private static void ResetFailedCount()
    {
        providerLock.Wait();
        FailedCount = 0;
        providerLock.Release();
    }
  }
}
