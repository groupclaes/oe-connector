using Microsoft.Extensions.Configuration;
using Progress.Open4GL.Proxy;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal class ProxyProvider : IProxyProvider
  {
    private readonly IConfigurationSection configurationSection;
    public ProxyProvider(IConfiguration configuration)
    {
      configurationSection = configuration.GetSection("OpenEdge");
    }

    public IProxyInterface CreateProxyInstance()

      => CreateProxyInstance("default", null, null, null);

    public IProxyInterface CreateProxyInstance(string appServer)
      => CreateProxyInstance(appServer, null, null, null);

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password)
      => CreateProxyInstance(appServer, userId, password, "");

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password,
      string appServerInfo)
    {
      AppServerConfig config = GetAppServerConfig(appServer);

      Connection connection = new Connection(config.Endpoint,
        userId ?? config.Username,
        password ?? config.Password,
        appServerInfo ?? config.AppId);

      return new PrefixedProxyInterface(connection, null);
    }

    public IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo, string procedurePrefix)
    {
      AppServerConfig config = GetAppServerConfig(appServer);

      Connection connection = new Connection(config.Endpoint,
        userId ?? config.Username,
        password ?? config.Password,
        appServerInfo ?? config.AppId);

      return new PrefixedProxyInterface(connection,
        procedurePrefix ?? config.PathPrefix);
    }

    private AppServerConfig GetAppServerConfig(string appServer)
    {
      AppServerConfig appConfig = new AppServerConfig();
      configurationSection.GetSection("Appservers")
        .Bind(appServer, appConfig);

      return appConfig;
    }
  }
}
