using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public class OpenEdge : IOpenEdge
  {
    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static readonly Random random = new Random();
    private const string CachePathPrefix = "OpenEdge:Procedures:";

    private readonly IDistributedCache cache;
    private readonly ILogger<OpenEdge> logger;

    public OpenEdge(IDistributedCache cache, ILogger<OpenEdge> logger)
    {
      this.cache = cache;
      this.logger = logger;
    }

    public async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
#if DEBUG
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
#endif
      byte[] rawData;
      string cacheName = $"{CachePathPrefix}{request.Procedure}";
      if (!string.IsNullOrEmpty(parameterHash))
      {
        cacheName += $":{parameterHash}";
      }
      logger.LogDebug("Using cache redis name {CacheKey}", cacheName);
      if (request.Cache < 1)
      {
        logger.LogInformation("Cache result {Found} for {Procedure}", "BYPASS", request.Procedure);
        return await GetProcedureResponseBytes(await GetProcedureResponse(request, cancellationToken).ConfigureAwait(false))
          .ConfigureAwait(false);
      }
      else
      {
        logger.LogDebug("Attempting to fetch cached response for {Procedure}", request.Procedure);
        rawData = await cache.GetAsync(cacheName, cancellationToken)
            .ConfigureAwait(false);

        if (rawData != null)
        {
          logger.LogInformation("Cache result {Found} for {Procedure}", "HIT", request.Procedure);
#if DEBUG
          stopwatch.Stop();
          logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif
          return rawData;
        }
        else
        {
          logger.LogTrace("Executing {Procedure} on OpenEdge", request.Procedure);
          ProcedureResponse result = await GetProcedureResponse(request, cancellationToken)
            .ConfigureAwait(false);
          logger.LogTrace("Executed {Procedure} on OpenEdge, result {@result}", request.Procedure, result);

          if (request.Cache > 0)
          {
            logger.LogDebug("Caching {Procedure} response for {Expire} seconds", request.Procedure, request.Cache);

            result.Retrieved = DateTime.UtcNow;
            rawData = await GetProcedureResponseBytes(result)
              .ConfigureAwait(false);

            _ = cache.SetAsync(cacheName, rawData,
              new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(request.Cache)
              }, cancellationToken).ConfigureAwait(false);
          }
          else
          {
            rawData = await GetProcedureResponseBytes(result)
              .ConfigureAwait(false);
          }

          logger.LogInformation("Cache result {Found} for {Procedure}", "MISS", request.Procedure);
#if DEBUG
          stopwatch.Stop();
          logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif
          return rawData;
        }
      }
    }


    public async Task<ProcedureResponse> GetProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
#if DEBUG
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
#endif
      byte[] rawData;
      string cacheName = $"{CachePathPrefix}{request.Procedure}";
      if (!string.IsNullOrEmpty(parameterHash))
      {
        cacheName += $":{parameterHash}";
      }
      logger.LogDebug("Using cache redis name {CacheKey}", cacheName);
      if (request.Cache < 1)
      {
        logger.LogInformation("Cache result {Found} for {Procedure}", "BYPASS", request.Procedure);
        return await GetProcedureResponse(request, cancellationToken)
          .ConfigureAwait(false);
      }
      else
      {
        logger.LogDebug("Attempting to fetch cached response for {Procedure}", request.Procedure);
        rawData = await cache.GetAsync(cacheName, cancellationToken)
            .ConfigureAwait(false);

        if (rawData != null)
        {
          logger.LogInformation("Cache result {Found} for {Procedure}", "HIT", request.Procedure);
#if DEBUG
          stopwatch.Stop();
          logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif
          return await GetProcedureFromBytes(rawData)
            .ConfigureAwait(false);
        }
        else
        {
          logger.LogTrace("Executing {Procedure} on OpenEdge", request.Procedure);
          ProcedureResponse result = await GetProcedureResponse(request, cancellationToken)
            .ConfigureAwait(false);
          logger.LogTrace("Executed {Procedure} on OpenEdge, result {@result}", request.Procedure, result);

          if (request.Cache > 0)
          {
            logger.LogDebug("Caching {Procedure} response for {Expire} seconds", request.Procedure, request.Cache);

            result.Retrieved = DateTime.UtcNow;
            rawData = await GetProcedureResponseBytes(result)
              .ConfigureAwait(false);

            _ = cache.SetAsync(cacheName, rawData,
              new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(request.Cache)
              }, cancellationToken).ConfigureAwait(false);
          }

          logger.LogInformation("Cache result {Found} for {Procedure}", "MISS", request.Procedure);
#if DEBUG
          stopwatch.Stop();
          logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif
          return result;
        }
      }
    }

    private Task<ProcedureResponse> GetProcedureResponse(ProcedureRequest request, CancellationToken cancellationToken)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      Dictionary<int, byte[]> outputsDictionary = new Dictionary<int, byte[]>();
      byte[] bytes;
      /*  //DEV IMPLEMENTATION */
      foreach (Parameter output in request.Parameters.Where(x => x.Output))
      {
        bytes = new byte[random.Next(1000, 65535)];
        random.NextBytes(bytes);

        outputsDictionary.Add(output.Position, bytes);
      }
      // await Task.Delay(random.Next(1000), cancellationToken)
      //   .ConfigureAwait(false);

      /* DEV IMPLEMENTATION// */

      stopwatch.Stop();
      logger.LogInformation("Execution time for {Procedure} was {ExecutionTime}", request.Procedure, stopwatch.ElapsedMilliseconds);

      ProcedureResponse response = new ProcedureResponse
      {
        Procedure = request.Procedure,
        Result = outputsDictionary,
        FetchTime = stopwatch.ElapsedMilliseconds
      };

      return Task.FromResult(response);
    }

    /// <summary>
    /// Check if any parameter should be redacted and return the displayeable version of the parameters
    /// </summary>
    /// <param name="request">Procedure request to parse and validate</param>
    /// <param name="displayeableFilters">New copy of parameters with the redacted version</param>
    /// <returns>Whether or not there is any redacted parameter</returns>
    public bool GetFilteredParameters(ProcedureRequest request, out Parameter[] displayeableFilters, out string parameterHash)
    {
      bool hasRedacted = false;
      displayeableFilters = new Parameter[request.Parameters.Length];

      StringBuilder hashBuilder = new StringBuilder(request.Parameters.Length * 64);
      for (int i = 0; i < request.Parameters.Length; i++)
      {
        if (!request.Parameters[i].Output)
        {
          if (request.Parameters[i].Value != null)
          {
            hashBuilder.AppendFormat("{0}{1}", request.Parameters[i].Position, request.Parameters[i].Value);
          }

          if (request.Parameters[i].Redact)
          {
            displayeableFilters[i] = request.Parameters[i].RedactCopy();
            hasRedacted = true;

            // Continue, as we don't need to add it as a normal parameter
            continue;
          }
        }

        displayeableFilters[i] = request.Parameters[i];
      };

      parameterHash = hashBuilder.Length > 0 ? Checksum.Generate(hashBuilder) : String.Empty;

      return hasRedacted;
    }

    private Task<byte[]> GetProcedureResponseBytes(ProcedureResponse response)
      => Task.Run(() => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes<ProcedureResponse>(response, serializerOptions));

    private Task<ProcedureResponse> GetProcedureFromBytes(byte[] bytes)
      => Task.Run(() => System.Text.Json.JsonSerializer.Deserialize<ProcedureResponse>(bytes, serializerOptions));
  }
}