using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public class OpenEdge : IOpenEdge
  {
    private const string CachePathPrefix = "OpenEdge:Procedures:";

    private readonly IDistributedCache cache;
    private readonly ILogger<OpenEdge> logger;

    public OpenEdge(IDistributedCache cache, ILogger<OpenEdge> logger)
    {
      this.cache = cache;
      this.logger = logger;
    }

    public async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, CancellationToken cancellationToken = default)
    {
      if (request.Cache == -1)
      {
        logger.LogInformation("Bypassing cache for {Procedure}", request.Procedure);
        return await GetProcedureResponse(request, cancellationToken);
      }
      else
      {
        logger.LogDebug("Attempting to fetch cached response for {Procedure}", request.Procedure);
        byte[] rawData = await cache.GetAsync(CachePathPrefix + request.Procedure, cancellationToken)
            .ConfigureAwait(false);

        if (rawData != null)
        {
          logger.LogInformation("Cache result {Found} for {Procedure}", "HIT", request.Procedure);
          return rawData;
        }
        else
        {
          logger.LogTrace("Executing {Procedure} on OpenEdge", request.Procedure);
          byte[] result = await GetProcedureResponse(request, cancellationToken);
          logger.LogTrace("Executed {Procedure} on OpenEdge, result {@result}", request.Procedure, result);
          if (request.Cache > 0)
          {
            logger.LogDebug("Caching {Procedure} response for {Expire} seconds", request.Cache);
            _ = cache.SetAsync(CachePathPrefix + request.Procedure, result,
              new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(request.Cache)
              }, cancellationToken);
          }

          logger.LogInformation("Cache result {Found} for {Procedure}", "MISS", request.Procedure);
          return result;
        }
      }
    }

    public async Task<byte[]> GetProcedureResponse(ProcedureRequest request, CancellationToken cancellationToken)
    {
      Stopwatch sw = new Stopwatch();
      sw.Start();

      sw.Stop();
      logger.LogInformation("Execution time for {Procedure} was {ExecutionTime}", sw.ElapsedMilliseconds);

      ProcedureResponse response = new ProcedureResponse
      {
        Age = -1,
        Procedure = request.Procedure,
        Result = null,
        ElapsedTime = sw.ElapsedMilliseconds
      };

      return await Task.Run(() => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<ProcedureResponse>(response))
        .ConfigureAwait(false);
    }
  }
}