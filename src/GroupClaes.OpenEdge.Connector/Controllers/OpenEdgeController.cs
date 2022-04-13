using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business;
using GroupClaes.OpenEdge.Connector.Business.Exceptions;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GroupClaes.OpenEdge.Connector.Controllers
{
  [ApiController, Route("api/[controller]")]
  public class OpenEdgeController : ControllerBase
  {
    private readonly ILogger<OpenEdgeController> logger;
    private readonly IOpenEdge openEdge;
    private readonly IParameterService parameterService;

    public OpenEdgeController(ILogger<OpenEdgeController> logger, IOpenEdge openEdge, IParameterService parameterService)
    {
      this.logger = logger;
      this.openEdge = openEdge;
      this.parameterService = parameterService;
    }

    [HttpPost("{procedure}/test")]
    [ProducesResponseType(typeof(ProcedureResponse), 200)]
    [ProducesResponseType(408)]
    [ProducesResponseType(500)]
    public Task<ActionResult> ExecuteProcedure([FromBody]ProcedureRequest request, string procedure)
      => ExecuteProcedure(request, procedure, true); 

    [HttpPost]
    [HttpPost("{procedure}")]
    [ProducesResponseType(typeof(ProcedureResponse), 200)]
    [ProducesResponseType(408)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> ExecuteProcedure([FromBody]ProcedureRequest request, string procedure, bool test = false)
    {
      if (!string.IsNullOrEmpty(procedure))
      {
        request.Procedure = procedure;
      }

      try
      {
#if DEBUG
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
#endif
        DisplayableParameter[] displayeableFilters = parameterService.GetFilteredParameters(request.Parameters, out bool hasRedacted, out string parameterHash);
        logger.LogInformation("{Connection}: Received procedure execute request for {Procedure} using {@InputParameter}",
          HttpContext.Connection.Id, request.Procedure, displayeableFilters);

        byte[] response = await openEdge.ExecuteProcedureWithTimeoutAsync(request, parameterHash, test, HttpContext.RequestAborted)
          .ConfigureAwait(false);
#if DEBUG
        stopwatch.Stop();
        logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif
        return File(response, "application/json");
      }
      catch (OpenEdgeTimeoutException)
      {
        return StatusCode(408);
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Couldn't execute and retrieve procedure {Procedure}", request.Procedure);
        return StatusCode(500);
      }
    }
  }
}