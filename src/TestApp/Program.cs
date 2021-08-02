using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Client;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        .AddLogging()
        .AddTransient<IOpenEdgeClient, OpenEdgeClient>()
        .AddSingleton<IConfiguration>(configuration)
        .BuildServiceProvider();

      await DoTests();

      Console.ReadLine();
    }

    private static async Task DoTests()
    {
      List<Parameter> parameters = new List<Parameter>
      {
        new Parameter
        {
          Output = true,
          Position = 1
        },
        new Parameter
        {
          Output = true,
          Position = 2
        }
      };

      List<TimeSpan> results = new List<TimeSpan>();
      List<IOpenEdgeClient> clients = new List<IOpenEdgeClient>();
      for (int j = 0; j < 16; j++)
      {
        IOpenEdgeClient openEdge = serviceProvider.GetRequiredService<IOpenEdgeClient>();

        // Authorize and let the service know our intentionts
        await openEdge.Authenticate("TestApp69", true);

        for (int i = 0; i < 300; i++)
        {
          Stopwatch stopWatch = new Stopwatch();
          stopWatch.Start();
          Action<ProcedureResponse> action = null;
          
          action = x => {
            stopWatch.Stop();
            Console.WriteLine("Procedure #1 {0}, {1}, {2}, {3}, Elapsed: {4}", x.Procedure, x.Retrieved, x.Status, openEdge.ConnectionId, stopWatch.Elapsed);
            results.Add(stopWatch.Elapsed);

            openEdge.UnregisterResponseHandler("test" + i, action);
          };
          openEdge.RegisterResponseHandler("test" + i, action);
        }

        clients.Add(openEdge);
      }

      for (int j = 0; j < 16; j++)
      {
        for (int i = 0; i < 300; i++)
        {
          _ = clients[j].ExecuteProcedure("test" + i, 100, parameters)
            .ConfigureAwait(false);
          Console.WriteLine("Procedure test {0} sent!", i);
        }
      }

      // for (int i = 0; i < 300; i++)
      // {
      //   Stopwatch stopWatch = new Stopwatch();
      //   stopWatch.Start();
      //   _ = clients[0].GetProcedure("test" + i, 500, parameters)
      //     .ContinueWith(x => {
      //       stopWatch.Stop();
      //       ProcedureResponse response = x.Result;
      //       Console.WriteLine("Procedure #2 {0}, {1}, {2}, {3}, Elapsed: {4}", response.Procedure, response.Retrieved, response.Status, clients[0].ConnectionId, stopWatch.Elapsed);
      //     });
      //   Console.WriteLine("Procedure test {0} sent!", i);
      // }

      
      foreach (var client in clients)
      {
        Console.WriteLine("Client ID: {0}", client.ConnectionId);
      }
    }
  }
}
