using GroupClaes.OpenEdge.Connector.Business.Raw;
using GroupClaes.OpenEdge.Connector.Shared;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System;

#if DEBUG
using System.Diagnostics;
#endif

namespace GroupClaes.OpenEdge.Connector.Business
{
  internal sealed class OpenEdgeWithCache : OpenEdge, IOpenEdge
  {
    private const string CachePathPrefix = "OpenEdge:Procedures:";
    private readonly IDistributedCache cache;

    public OpenEdgeWithCache(IDistributedCache cache, ILogger<OpenEdge> logger, IProxyProvider proxyProvider,
      IParameterService parameterService, IProcedureParser procedureParser)
      : base(logger, proxyProvider, parameterService, procedureParser)
    {
      this.cache = cache;
    }

    public override async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null,
      bool isTest = false, CancellationToken cancellationToken = default)
    {
      if (request.Cache > 0)
      {
#if DEBUG
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
#endif
        byte[] result = await GetAndCacheProcedureResponse(request, parameterHash, cancellationToken)
          .ConfigureAwait(false);
#if DEBUG
        stopwatch.Stop();
        logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif
        return result;
      }
      else
      {
        return await base.ExecuteProcedureAsync(request, parameterHash, isTest, cancellationToken);
      }
    }

    internal async Task<byte[]> GetAndCacheProcedureResponse(ProcedureRequest request, string parameterHash,
      CancellationToken cancellationToken = default)
    {
      byte[] oeResult;
      string cacheName = GetCachedKey(request.Procedure, parameterHash);
      logger.LogDebug("Attempting to fetch cached response for {Procedure}: {ParameterHash}",
        request.Procedure, parameterHash);
      try
      {
        oeResult = await cache.GetAsync(cacheName, cancellationToken)
          .ConfigureAwait(false);
        if (oeResult != null)
        {
          logger.LogInformation("Cache result {Found} for {Procedure}: {ParameterHash}", "HIT",
            request.Procedure, parameterHash);
          return oeResult;
        }
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
        logger.LogError(ex, "Couldn't fetch cached response for {Procedure}: {ParameterHash}",
          request.Procedure, parameterHash);
      }

      logger.LogTrace("Executing {Procedure} on OpenEdge", request.Procedure);
      ProcedureResponse result = await GetProcedureResponse(request, cancellationToken);
      logger.LogTrace("Executed {Procedure} on OpenEdge, result {@result}", request.Procedure, result);

      result.LastModified = DateTime.UtcNow;
      oeResult = procedureParser.GetProcedureResponseBytes(result);
      _ = WriteProcedureResponseToCache(cacheName, request, oeResult, cancellationToken);

      logger.LogInformation("Cache result {Found} for {Procedure}", "MISS", request.Procedure);
      return oeResult;
    }

    /// <summary>
    /// Write the procedure response to the redis cache
    /// </summary>
    /// <param name="cacheName"></param>
    /// <param name="request"></param>
    /// <param name="resultBytes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal Task WriteProcedureResponseToCache(string cacheName, ProcedureRequest request,
      byte[] resultBytes, CancellationToken cancellationToken = default)
    {
      logger.LogDebug("Caching {Procedure} response for {Expire} milliseconds", request.Procedure, request.Cache);

      return cache.SetAsync(cacheName, resultBytes,
        new DistributedCacheEntryOptions
        {
          AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(request.Cache)
        }, cancellationToken);
    }

    /// <summary>
    /// Get the cache key based upon the procedure and the parameter hash
    /// </summary>
    /// <param name="requestProcedure">Procedure name to include in the key</param>
    /// <param name="parameterHash">Parameter hash to differentiate requested datas</param>
    /// <returns>The key to be used to access from the cache</returns>
    private static string GetCachedKey(string requestProcedure, string parameterHash)
    {
      if (!string.IsNullOrEmpty(parameterHash))
      {
        return $"{CachePathPrefix}{requestProcedure}:{parameterHash}";
      }

      return $"{CachePathPrefix}{requestProcedure}";
    }
  }
}
