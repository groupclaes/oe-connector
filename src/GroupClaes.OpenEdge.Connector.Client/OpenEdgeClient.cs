using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace GroupClaes.OpenEdge.Connector.Client
{
  public class OpenEdgeClient : IOpenEdgeClient
  {
    public string ConnectionId { get => connection.ConnectionId; }

    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ILogger<OpenEdgeClient> logger;
    private readonly HubConnection connection;
    private readonly ConcurrentDictionary<string, List<Action<ProcedureResponse>>> responseHandlers;
    private readonly List<Action<ProcedureResponse>> genericHandlers;

    private bool isDisposing = false;

    public OpenEdgeClient(ILogger<OpenEdgeClient> logger, IConfiguration configuration)
    {
      this.logger = logger;
      connection = new HubConnectionBuilder()
        .WithUrl(configuration["OpenEdge:Endpoint"])
        .WithAutomaticReconnect()
        .AddMessagePackProtocol()
        .ConfigureLogging(x => x.AddJsonConsole())
        .Build();
      responseHandlers = new ConcurrentDictionary<string, List<Action<ProcedureResponse>>>();
      genericHandlers = new List<Action<ProcedureResponse>>();

      connection.On<byte[]>("ProcedureResponse", array => Task.Run(() => {
        ProcedureResponse response = JsonSerializer.Deserialize<ProcedureResponse>(array, serializerOptions);

        if (responseHandlers.TryGetValue(response.Procedure, out var list) && list.Count > 0)
        {
          lock (list)
          {
            foreach (var action in list)
            {
              action(response);
            }
          }
        }

        if (genericHandlers.Count > 0)
        {
          lock (genericHandlers)
          {
            foreach (var action in genericHandlers)
            {
              action(response);
            }
          }
        }
      }));
    }

    public async Task ExecuteProcedure(string procedureName, int cacheTime, IEnumerable<Parameter> parameters)
    {
      ProcedureRequest request = new ProcedureRequest
      {
        Procedure = procedureName,
        Parameters = parameters.ToArray(),
        Cache = cacheTime
      };

      await connection.SendAsync("ExecuteProcedure", request)
        .ConfigureAwait(false);
    }

    public async Task<ProcedureResponse> GetProcedure(string procedureName, int cacheTime, IEnumerable<Parameter> parameters)
    {
      ProcedureRequest request = new ProcedureRequest
      {
        Procedure = procedureName,
        Parameters = parameters.ToArray(),
        Cache = cacheTime
      };

      return await connection.InvokeAsync<ProcedureResponse>("GetProcedure", request)
        .ConfigureAwait(false);
    }

    public async Task Authenticate(string application, bool test)
    {
      await CheckConnection()
        .ConfigureAwait(false);

      ConnectionRequest request = new ConnectionRequest
      {
        Application = application,
        Test = test
      };

      await connection.SendAsync("Authenticate", request)
        .ConfigureAwait(false);
    }

    public void RegisterResponseHandler(string procedureName, Action<ProcedureResponse> action)
    {
      List<Action<ProcedureResponse>> list = responseHandlers.GetOrAdd(procedureName, new List<Action<ProcedureResponse>>());

      list.Add(action);
    }
    
    public void RegisterResponseHandler(Action<ProcedureResponse> action)
      => genericHandlers.Add(action);

    public void UnregisterResponseHandler(string procedureName, Action<ProcedureResponse> action)
    {
      List<Action<ProcedureResponse>> list = responseHandlers.GetOrAdd(procedureName, new List<Action<ProcedureResponse>>());

      list.Remove(action);
    }
    
    public void UnregisterResponseHandler(Action<ProcedureResponse> action)
      => genericHandlers.Remove(action);

    public Task CheckConnection()
    {
      if (connection.State != HubConnectionState.Connected && connection.State != HubConnectionState.Connecting)
      {
        return connection.StartAsync();
      }

      return Task.CompletedTask;
    }
  }
}
