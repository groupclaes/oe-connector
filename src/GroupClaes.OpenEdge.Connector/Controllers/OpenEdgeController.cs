using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroupClaes.OpenEdge.Connector.Controllers
{
  [ApiController, Route("[controller]")]
  public class OpenEdgeController : ControllerBase
  {
    private readonly ILogger<OpenEdgeController> logger;
    private readonly IOpenEdge openEdge;

    public OpenEdgeController(ILogger<OpenEdgeController> logger, IOpenEdge openEdge)
    {
      this.logger = logger;
      this.openEdge = openEdge;
    }

    [HttpGet]
    public async Task<ActionResult<byte[]>> ExecuteProcedure(ProcedureRequest request)
    {
      IEnumerable<Parameter> displayeableFilters = request.Parameters.Select(x =>
      {
        if (x.Redact)
        {
          x.Value = "***";
        }

        return x;
      });

      logger.LogInformation("{Connection}: Received procedure execute request for {Procedure} using {@Parameters}",
        HttpContext.Connection.Id, request.Procedure, displayeableFilters);

      byte[] response = await openEdge.ExecuteProcedureAsync(request, HttpContext.RequestAborted);
      return Ok(response);
    }
  }
}