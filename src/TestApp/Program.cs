using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Client;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestApp
{
  class Program
  {
    private static IServiceProvider serviceProvider;
    
    static async Task Main(string[] args)
    {
      IConfiguration configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
          { "OpenEdge:Endpoint", "http://localhost:5000/openedge" }
        })
        .Build();

      serviceProvider = new ServiceCollection()
        .AddLogging(x => x.AddConsole())
        .AddTransient<IOpenEdgeClient, OpenEdgeClient>()
        .AddSingleton<IConfiguration>(configuration)
        .BuildServiceProvider();

      await DoTests(serviceProvider.GetRequiredService<ILogger<Program>>());

      Console.ReadLine();
    }

    private static async Task DoTests(ILogger<Program> logger)
    {
      List<Parameter> parameters = new List<Parameter>
      {
        new Parameter
        {
          Output = true,
          Position = 1,
          Value = JsonSerializer.SerializeToElement(new { Cheese = true })
        },
        new Parameter
        {
          Output = true,
          Position = 2
        },
        new Parameter
        {
          Output = true,
          Position = 3
        }
      };

      List<TimeSpan> results = new List<TimeSpan>();
      List<IOpenEdgeClient> clients = new List<IOpenEdgeClient>();
      for (int j = 0; j < 300; j++)
      {
        IOpenEdgeClient openEdge = serviceProvider.GetRequiredService<IOpenEdgeClient>();

        // Authorize and let the service know our intentionts
        await openEdge.Authenticate("TestApp69", true);

        for (int i = 0; i < 16; i++)
        {
          Stopwatch stopWatch = new Stopwatch();
          stopWatch.Start();
          Action<ProcedureResponse> action = null;
          int y = j;
          
          action = x => {
            stopWatch.Stop();
            logger.LogInformation("Procedure #{id} {Procedure}, {LastModified}, {Status}, {Connectionid}, Elapsed: {Elapsed}", y, x.Procedure, x.LastModified, x.Status, openEdge.ConnectionId, stopWatch.Elapsed);
            results.Add(stopWatch.Elapsed);

            openEdge.UnregisterResponseHandler("test" + i, action);
          };
          openEdge.RegisterResponseHandler("test" + i, action);
        }

        clients.Add(openEdge);
        logger.LogInformation("Client ID: {ConnectionId}", openEdge.ConnectionId);
      }

      for (int j = 0; j < 300; j++)
      {
        IOpenEdgeClient client = clients[j];
        _ = Task.Run(() => {
          for (int i = 0; i < 16; i++)
          {
            _ = client.ExecuteProcedure("test" + i, 100, parameters);
            logger.LogInformation("Procedure test {Id} sent!", i);
          }
        }).ConfigureAwait(false);
      }

      for (int i = 0; i < 300; i++)
      {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        _ = clients[i].GetProcedure("test" + i, 500, parameters)
          .ContinueWith(x => {
            stopWatch.Stop();
            ProcedureResponse response = x.Result;
            logger.LogInformation("Procedure #2 {Procedure}, {LastModified}, {Status}, {Connectionid}, Elapsed: {Elapsed}", response.Procedure, response.LastModified, response.Status, clients[0].ConnectionId, stopWatch.Elapsed);
          });
        logger.LogInformation("Procedure test {Id} sent!", i);
      }
    }
  }
}
