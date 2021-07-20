using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GroupClaes.OpenEdge.Connector.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class OpenEdgeHub : Hub
  {
    private readonly ILogger<OpenEdgeHub> logger;
    private readonly IOpenEdge openEdge;

    public OpenEdgeHub(ILogger<OpenEdgeHub> logger, IOpenEdge openEdge)
    {
      this.logger = logger;
      this.openEdge = openEdge;
    }

    public async Task ExecuteProcedure(ProcedureRequest request)
    {
      IEnumerable<Parameter> displayeableFilters = request.Parameters.Select(x => {
        if (x.Redact) {
          x.Value = "***";
        }

        return x;
      });

      logger.LogInformation("{Connection}: Received procedure execute request for {Procedure} using {@Parameters}",
        Context.ConnectionId, request.Procedure, displayeableFilters);

      byte[] response = await openEdge.ExecuteProcedureAsync(request, Context.ConnectionAborted);
      _ = Clients.Caller.SendAsync("ProcedureResponse", response, Context.ConnectionAborted);
    }
  }
}
