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
    static async Task Main(string[] args)
    {
      IConfiguration configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
          { "OpenEdge:Endpoint", "http://localhost:5000/openedge" }
        })
        .Build();

      IServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddScoped<IOpenEdgeClient, OpenEdgeClient>()
        .AddSingleton<IConfiguration>(configuration)
        .BuildServiceProvider();

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

        // Preconnect hek
        await openEdge.Authenticate("TestApp69", true);

        for (int i = 0; i < 300; i++)
        {
          Stopwatch stopWatch = new Stopwatch();
          stopWatch.Start();
          Action<ProcedureResponse> action = null;
          
          action = x => {
            stopWatch.Stop();
            Console.WriteLine("Procedure {0}, {1}, {2}, Elapsed: {3}", x.Procedure, x.Retrieved, x.Status, stopWatch.Elapsed);
            results.Add(stopWatch.Elapsed);

            openEdge.UnregisterResponseHandler("test" + i, action);
          };
          openEdge.RegisterResponseHandler("test" + i, action);
        }

        clients.Add(openEdge);
      }

      for (int i = 0; i < 300; i++)
      {
        _ = clients[0].ExecuteProcedure("test" + i, 100, parameters)
          .ConfigureAwait(false);
        Console.WriteLine("Procedure test {0} sent!", i);
      }

      for (int i = 0; i < 300; i++)
      {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        _ = clients[0].GetProcedure("test_" + i, 500, parameters)
          .ContinueWith(x => {
            stopWatch.Stop();
            ProcedureResponse response = x.Result;
            Console.WriteLine("Procedure {0}, {1}, {2}, Elapsed: {3}", response.Procedure, response.Retrieved, response.Status, stopWatch.Elapsed);
          });
        Console.WriteLine("Procedure test {0} sent!", i);
      }

      Console.ReadLine();
    }
  }
}
