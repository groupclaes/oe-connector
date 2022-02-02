using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business;
using GroupClaes.OpenEdge.Connector.Models;
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
    private readonly IParameterService parameterService;

    private readonly ConcurrentDictionary<string, ProcedureGroup> procedureTimers;
    private bool IsTesting { get => (bool)Context.Items[IsTest]; }

    public OpenEdgeHub(ILogger<OpenEdgeHub> logger, IOpenEdge openEdge, IParameterService parameterService)
    {
      this.logger = logger;
      this.openEdge = openEdge;
      this.parameterService = parameterService;
      this.procedureTimers = new ConcurrentDictionary<string, ProcedureGroup>();
    }

    public void Authenticate(ConnectionRequest request)
    {
      logger.LogInformation("Connection request received for {Connection} identifying as microservice {Application} using test {Test}",
        Context.ConnectionId, request.Application, request.Test);

      Context.Items[IsTest] = request.Test;
    }

    public async Task ExecuteProcedure(ProcedureRequest request)
    {
      Parameter[] displayeableParameters = parameterService.GetFilteredParameters(request.Parameters, out bool hasRedacted, out string parameterHash);
      logger.LogInformation("{Connection}: Received procedure execute request for {Procedure} using {@Parameters}",
        Context.ConnectionId, request.Procedure, hasRedacted ? displayeableParameters : request.Parameters);

      if (!hasRedacted && request.Cache > 0)
      {
        ProcedureGroup group = GetProcedureGroup(request.Procedure, parameterHash);
        group.Initialize();
        group.AddRecipient(Context.ConnectionId);

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
      Parameter[] displayeableParameters = parameterService.GetFilteredParameters(request.Parameters, out bool hasRedacted, out string parameterHash);
      logger.LogInformation("{Connection}: Received procedure retrieval request for {Procedure} using {@Parameters}",
        Context.ConnectionId, request.Procedure, hasRedacted ? displayeableParameters : request.Parameters);
      
      // Get procedure response.
      // If has none redacted and has a cache, send to all current subscribers/awaiting people
      ProcedureResponse response = await openEdge.GetProcedureAsync(request, parameterHash, IsTesting, Context.ConnectionAborted)
        .ConfigureAwait(false);

      return response;
    }

    private async Task ExecuteAndSendToGroup(ProcedureRequest request, string parameterHash)
    {
      byte[] response = await openEdge.ExecuteProcedureAsync(request, parameterHash, IsTesting, Context.ConnectionAborted)
        .ConfigureAwait(false);

      logger.LogDebug("Received procedure {Procedure} response {BytesLength}", request.Procedure, response.Length);
      
      ProcedureGroup group = GetProcedureGroup(request.Procedure, parameterHash);
      logger.LogDebug("Sending procedure {Procedure} response to {@Recipients}", request.Procedure, group.Recipients);
      await Clients.Clients(group.Recipients).SendAsync("ProcedureResponse", response)
        .ConfigureAwait(false);
      logger.LogInformation("Sent response for {Procedure} to {@Recipients}, total time taken: {TimeTaken}", request.Procedure, group.Recipients, group.Stopwatch.Elapsed);
    }

    private async Task ExecuteAndSendToClient(ProcedureRequest request, string parameterHash)
    {
      byte[] response = await openEdge.ExecuteProcedureAsync(request, parameterHash, IsTesting, Context.ConnectionAborted)
        .ConfigureAwait(false);

      await Clients.Caller.SendAsync("ProcedureResponse", response, Context.ConnectionAborted)
        .ConfigureAwait(false);
    }

    private ProcedureGroup GetProcedureGroup(string procedure, string parameterHash)
      => this.procedureTimers.GetOrAdd($"{parameterHash}{procedure}{IsTesting}", proc => new ProcedureGroup(proc));
  }
}
