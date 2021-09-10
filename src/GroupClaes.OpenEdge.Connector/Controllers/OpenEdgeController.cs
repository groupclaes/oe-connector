#if DEBUG
using System.Diagnostics;
#endif
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroupClaes.OpenEdge.Connector.Controllers
{
  [ApiController, Route("api/[controller]")]
  public class OpenEdgeController : ControllerBase
  {
    private readonly ILogger<OpenEdgeController> logger;
    private readonly IOpenEdge openEdge;

    public OpenEdgeController(ILogger<OpenEdgeController> logger, IOpenEdge openEdge)
    {
      this.logger = logger;
      this.openEdge = openEdge;
    }

    [HttpPost("{procedure}/test")]
    public Task<ActionResult<byte[]>> ExecuteProcedure([FromBody]ProcedureRequest request, string procedure)
      => ExecuteProcedure(request, procedure, true); 

    [HttpPost]
    [HttpPost("{procedure}")]
    public async Task<ActionResult<byte[]>> ExecuteProcedure([FromBody]ProcedureRequest request, string procedure, bool test = false)
    {
#if DEBUG
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
#endif
      if (!string.IsNullOrEmpty(procedure))
      {
        request.Procedure = procedure;
      }

      bool hasRedacted = openEdge.GetFilteredParameters(request, out Parameter[] displayeableFilters, out string parameterHash);
      logger.LogInformation("{Connection}: Received procedure execute request for {Procedure} using {@Parameters}",
        HttpContext.Connection.Id, request.Procedure, displayeableFilters);

      byte[] response = await openEdge.ExecuteProcedureAsync(request, parameterHash, test, HttpContext.RequestAborted)
        .ConfigureAwait(false);
#if DEBUG
      stopwatch.Stop();
      logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif
      return File(response, "application/json");
    }
  }
}