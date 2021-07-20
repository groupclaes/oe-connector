using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GroupClaes.OpenEdge.Connector.Client
{
  public class OpenEdgeClient : IOpenEdgeClient
  {
    private readonly ILogger<OpenEdgeClient> logger;
    private readonly HubConnection connection;

    public OpenEdgeClient(ILogger<OpenEdgeClient> logger, IConfiguration configuration)
    {
      this.logger = logger;
      connection = new HubConnectionBuilder()
        .WithUrl(configuration["OpenEdge:Endpoint"])
        .WithAutomaticReconnect()
        .Build();
    }

    public Task ExecuteProcedure(string procedureName, IEnumerable<Parameter> parameters)
    {
      ProcedureRequest request = new ProcedureRequest
      {
        Procedure = procedureName,
        Parameters = parameters.ToArray()
      };

      return connection.InvokeAsync("ExecuteProcedure", request);
    }

    public Task<ProcedureResponse> GetProcedure(string procedureName, IEnumerable<Parameter> parameters)
    {
      ProcedureRequest request = new ProcedureRequest
      {
        Procedure = procedureName,
        Parameters = parameters.ToArray()
      };

      return connection.InvokeAsync<ProcedureResponse>("ExecuteProcedure", request);
    }
  }
}
