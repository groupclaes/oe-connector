using GroupClaes.OpenEdge.Connector.Business.Raw;
using Microsoft.Extensions.DependencyInjection;
using Progress.Open4GL.Proxy;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public static class OpenEdgeServiceCollectionExtensions
  {
    public static IServiceCollection AddOpenEdge(this IServiceCollection collection,
        string url, string userId, string password, string appId)
    {
      return collection.AddScoped<IOpenEdge, OpenEdge>()
        .AddScoped<IProxyInterface>(x => new ProxyInterface(new Connection(url, userId, password, appId)));
    }
  }
}