using System.Text;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GroupClaes.OpenEdge.Connector.Hubs
{
  public class OpenEdgeHub : Hub
  {
    private const string IsTest = "is_test";

    private readonly ILogger<OpenEdgeHub> logger;
    private readonly IOpenEdge openEdge;

    private bool IsTesting { get => (bool)Context.Items[IsTest]; }

    public OpenEdgeHub(ILogger<OpenEdgeHub> logger, IOpenEdge openEdge)
    {
      this.logger = logger;
      this.openEdge = openEdge;
    }

    public Task Authenticate(ConnectionRequest request)
    {
      logger.LogInformation("Connection request received for {Connection} identifying as microservice {Application} using test {Test}",
        Context.ConnectionId, request.Application, request.Test);

      Context.Items[IsTest] = request.Test;

      return Task.CompletedTask;
    }

    public async Task ExecuteProcedure(ProcedureRequest request)
    {
      bool hasRedacted = openEdge.GetFilteredParameters(request, out Parameter[] displayeableFilters, out string parameterHash);
      logger.LogInformation("{Connection}: Received procedure execute request for {Procedure} using {@Parameters}",
        Context.ConnectionId, request.Procedure, hasRedacted ? displayeableFilters : request.Parameters);

      if (!hasRedacted && request.Cache > 0)
      {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetProcedureName(request.Procedure), Context.ConnectionAborted)
          .ConfigureAwait(false);
        // Nothing is redacted meaning nothing is confidential/personal so we can cache the data and send it to all recipients
        await ExecuteAndSendToGroup(request, parameterHash)
          .ConfigureAwait(false);
      }
      else
        await ExecuteAndSendToClient(request, parameterHash)
          .ConfigureAwait(false);
    }

    public async Task<ProcedureResponse> GetProcedure(ProcedureRequest request)
    {
      bool hasRedacted = openEdge.GetFilteredParameters(request, out Parameter[] displayeableFilters, out string parameterHash);
      logger.LogInformation("{Connection}: Received procedure retrieval request for {Procedure} using {@Parameters}",
        Context.ConnectionId, request.Procedure, hasRedacted ? displayeableFilters : request.Parameters);
      
      // Get procedure response.
      // If has none redacted and has a cache, send to all current subscribers/awaiting people

      ProcedureResponse response = await openEdge.GetProcedureAsync(request, parameterHash, IsTesting, Context.ConnectionAborted)
        .ConfigureAwait(false);
      // Nothing is redacted meaning nothing is confidential/personal so we can cache the data and send it to all recipients
      return response;
    }

    private async Task ExecuteAndSendToGroup(ProcedureRequest request, string parameterHash)
    {
      byte[] response = await openEdge.ExecuteProcedureAsync(request, parameterHash, IsTesting, Context.ConnectionAborted)
        .ConfigureAwait(false);

      logger.LogDebug("Received procedure {Procedure} response {BytesLength}", request.Procedure, response.Length);
      await GetProcedureGroup(request.Procedure).SendAsync("ProcedureResponse", response)
        .ConfigureAwait(false);
    }

    private async Task ExecuteAndSendToClient(ProcedureRequest request, string parameterHash)
    {
      byte[] response = await openEdge.ExecuteProcedureAsync(request, parameterHash, IsTesting, Context.ConnectionAborted)
        .ConfigureAwait(false);

      await Clients.Caller.SendAsync("ProcedureResponse", response, Context.ConnectionAborted)
        .ConfigureAwait(false);
    }

    private IClientProxy GetProcedureGroup(string procedure)
      => Clients.Group(GetProcedureName(procedure));

    /// <summary>
    /// Get a procedure name 
    /// </summary>
    /// <param name="procedure"></param>
    /// <returns></returns>
    private string GetProcedureName(string procedure)
      => IsTesting ? $"procedure_test_{procedure}" : $"procedure_{procedure}";
  }
}
