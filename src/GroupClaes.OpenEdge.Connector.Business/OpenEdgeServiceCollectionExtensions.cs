using Microsoft.Extensions.DependencyInjection;

namespace GroupClaes.OpenEdge.Connector.Business
{
    public static class OpenEdgeServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenEdge(this IServiceCollection collection)
            => collection.AddScoped<IOpenEdge, OpenEdge>();
    }
}