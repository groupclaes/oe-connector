using GroupClaes.OpenEdge.Connector.Business.Raw;
using Microsoft.Extensions.DependencyInjection;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public static class OpenEdgeServiceCollectionExtensions
  {
    public static IServiceCollection AddOpenEdge(this IServiceCollection collection)
    {
      return collection.AddSingleton<IChecksumService, ChecksumService>()
        .AddSingleton<IProxyProvider, ProxyProvider>()
        .AddSingleton<IProcedureParser, ProcedureParser>()
        .AddSingleton<IJsonSerializer, JsonSerializer>()
        .AddSingleton<IParameterService, ParameterService>()
        .AddSingleton<TracerLogger>()
        .AddScoped<IOpenEdge, OpenEdge>();
    }
  }
}