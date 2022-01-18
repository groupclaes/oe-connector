using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace GroupClaes.OpenEdge.Connector
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(ConfigureElasticSink(context.Configuration)))
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });

    private static ElasticsearchSinkOptions ConfigureElasticSink(IConfiguration configuration)
    {
      return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
      {
        TypeName = null,
        IndexFormat = $"logs-aspnet-production",
        AutoRegisterTemplate = false,
        BatchAction = ElasticOpType.Create,
        EmitEventFailure = EmitEventFailureHandling.ThrowException
      };
    }
  }
}
