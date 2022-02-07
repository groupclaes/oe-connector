using GroupClaes.OpenEdge.Connector.Business.Raw;
using Microsoft.Extensions.DependencyInjection;

namespace GroupClaes.OpenEdge.Connector.Business.Extensions
{
  public static class OpenEdgeServiceCollectionExtensions
  {
    public static IServiceCollection AddOpenEdge(this IServiceCollection collection)
    {
      return collection.AddOpenEdge(false);
    }
    public static IServiceCollection AddOpenEdge(this IServiceCollection collection, bool withCache)
    {
      collection.AddOpenEdgeGenerics();

      if (withCache)
      {
        return collection.AddScoped<IOpenEdge, OpenEdgeWithCache>();
      }
      else
      {
        return collection.AddScoped<IOpenEdge, OpenEdge>();
      }
    }

    public static IServiceCollection AddOpenEdgeGenerics(this IServiceCollection collection)
    {
      return collection.AddSingleton<IChecksumService, ChecksumService>()
        .AddSingleton<IProxyProvider, ProxyProvider>()
        .AddSingleton<IProcedureParser, ProcedureParser>()
        .AddSingleton<IJsonSerializer, JsonSerializer>()
        .AddSingleton<IParameterService, ParameterService>()
        .AddSingleton<TracerLogger>();
    }
  }
}