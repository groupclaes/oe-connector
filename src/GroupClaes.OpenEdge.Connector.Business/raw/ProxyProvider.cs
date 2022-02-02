using GroupClaes.OpenEdge.Connector.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Progress.Open4GL.Proxy;
using System;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal class ProxyProvider : IProxyProvider
  {
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
      AppServerConfig config = GetAppServerConfig(appServer);

      SetTraceLogger();
      Connection connection = new Connection(config.Endpoint,
        userId ?? config.Username,
        password ?? config.Password,
        appServerInfo ?? config.AppId);

      config.Password = null;

      logger.LogDebug("Retrieved app server config for {Endpoint} with config {@Config}", config.Endpoint, config);

      return new ProxyInterface(GetLogger<PrefixedProxyInterface>(), connection);
    }

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo, string procedurePrefix)
    {
      AppServerConfig config = GetAppServerConfig(appServer);

      SetTraceLogger();
      Connection connection = new Connection(config.Endpoint,
        userId ?? config.Username,
        password ?? config.Password,
        appServerInfo ?? config.AppId);

      return new PrefixedProxyInterface(GetLogger<PrefixedProxyInterface>(), connection,
        procedurePrefix ?? config.PathPrefix);
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
  }
}
