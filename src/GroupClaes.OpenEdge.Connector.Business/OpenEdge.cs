using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business.Raw;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Progress.Open4GL.DynamicAPI;
using RAW;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public class OpenEdge : IOpenEdge
  {
    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const string CachePathPrefix = "OpenEdge:Procedures:";

    private readonly IDistributedCache cache;
    private readonly ILogger<OpenEdge> logger;
    private readonly IProxyInterface proxyInterface;

    public OpenEdge(IDistributedCache cache, ILogger<OpenEdge> logger, IProxyInterface proxyInterface)
    {
      this.cache = cache;
      this.logger = logger;
      this.proxyInterface = proxyInterface;
    }

    public async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
#if DEBUG
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
#endif
      byte[] rawData = null;
      string cacheName = $"{CachePathPrefix}{request.Procedure}";
      if (!string.IsNullOrEmpty(parameterHash))
      {
        cacheName += $":{parameterHash}";
      }
      logger.LogDebug("Using cache redis name {CacheKey}", cacheName);
      if (request.Cache < 1)
      {
        logger.LogInformation("Cache result {Found} for {Procedure}", "BYPASS", request.Procedure);
        return GetProcedureResponseBytes(await GetProcedureResponse(request, cancellationToken).ConfigureAwait(false));
      }
      else
      {
        logger.LogDebug("Attempting to fetch cached response for {Procedure}", request.Procedure);
        try
        {
          rawData = await cache.GetAsync(cacheName, cancellationToken)
            .ConfigureAwait(false);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
          logger.LogError(ex, "Couldn't fetch cached response for {Procedure}", request.Procedure);
        }

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

            result.LastModified = DateTime.UtcNow;
            rawData = GetProcedureResponseBytes(result);

            _ = cache.SetAsync(cacheName, rawData,
              new DistributedCacheEntryOptions {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(request.Cache)
              }, cancellationToken).ConfigureAwait(false);
          }
          else
          {
            rawData = GetProcedureResponseBytes(result);
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
          return GetProcedureFromBytes(rawData);
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

            result.LastModified = DateTime.UtcNow;
            rawData = GetProcedureResponseBytes(result);

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
      return Task.Run(() =>
      {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        ParameterSet parameters = GenerateParameterSet(request.Parameters);

        cancellationToken.ThrowIfCancellationRequested();

        proxyInterface.RunProcedure(request.Procedure, parameters);
        cancellationToken.ThrowIfCancellationRequested();
        Dictionary<int, object> outputsDictionary = GetOutputParameters(request.Parameters, parameters);

        stopwatch.Stop();
        logger.LogInformation("Execution time for {Procedure} was {ExecutionTime}", request.Procedure, stopwatch.ElapsedMilliseconds);

        if (outputsDictionary.All(x => x.Value != null))
        {
          return new ProcedureResponse()
          {
            Status = 200,
            Procedure = request.Procedure,
            Result = outputsDictionary,
            OriginTime = stopwatch.ElapsedMilliseconds
          };
        }
        else
        {
          logger.LogWarning("Executed {Procedure} but received one or more null fields.", request.Procedure);
          return new ProcedureResponse()
          {
            Status = 500,
            Procedure = request.Procedure,
            Result = null,
            OriginTime = stopwatch.ElapsedMilliseconds
          };
        }
      }, cancellationToken);
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

      parameterHash = hashBuilder.Length > 0 ? Checksum.Generate(hashBuilder) : string.Empty;

      return hasRedacted;
    }

    private static ParameterSet GenerateParameterSet(IEnumerable<Parameter> parameters)
    {
      ParameterSet parameterSet = new ParameterSet(parameters.Count());
      foreach (Parameter parameter in parameters)
      {
        if (parameter.Value != null)
        {
          int inputOutputType = parameter.Output ? ParameterSet.OUTPUT : ParameterSet.INPUT;
          JsonElement value = (JsonElement)parameter.Value;
          if (value.ValueKind == JsonValueKind.Array)
          {
            object[] values = value.EnumerateArray()
              .Select(x => x.GetRawText())
              .ToArray();
            parameterSet.setParameter(parameter.Position, values, inputOutputType, GetParameterSetType(parameter.Type), (values.Length > 0), values.Length, null);
          }
          else
          {
            parameterSet.setParameter(parameter.Position, value.GetString(), inputOutputType, GetParameterSetType(parameter.Type), false, 0, null);
          }
        }
        else
        {
          parameterSet.setParameter(parameter.Position, null, ParameterSet.OUTPUT, GetParameterSetType(parameter.Type), false, 0, null);
        }
      }

      return parameterSet;
    }
    
    /// <summary>
    /// Get a Progress compliant parametertype integer.
    /// </summary>
    /// <param name="type">Parametertype to convert</param>
    /// <returns></returns>
    private static int GetParameterSetType(ParameterType type)
    {
      if (type == ParameterType.JSON)
      {
        return (int)ParameterType.MemPointer;
      }
      else
      {
        return (int)type;
      }
    }

    private Dictionary<int, object> GetOutputParameters(Parameter[] requestParameters, ParameterSet parameters)
      => requestParameters.Where(x => x.Output)
          .ToDictionary(x => x.Position, x =>
          { 
            // We have to get it out of the parameter set because Progress... Grrrrr....
            object value = parameters.getOutputParameter(x.Position);
            if (value is Progress.Open4GL.Memptr pointer)
            {
              if (x.Type == ParameterType.JSON)
              {
                return JsonDocument.Parse(pointer.Bytes);
              }

              return pointer.Bytes;
            }
            else if (value is string result && x.Type == ParameterType.LongChar)
            {
              return JsonDocument.Parse(result);
            }

            return value;
          });

    private static byte[] GetProcedureResponseBytes(ProcedureResponse response)
      => JsonSerializer.SerializeToUtf8Bytes(response, serializerOptions);

    private static ProcedureResponse GetProcedureFromBytes(byte[] bytes)
      => JsonSerializer.Deserialize<ProcedureResponse>(bytes, serializerOptions);
  }
}