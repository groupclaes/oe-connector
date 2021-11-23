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
using Microsoft.Extensions.Logging;
using Progress.Open4GL.DynamicAPI;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public class OpenEdge : IOpenEdge
  {
    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const string CachePathPrefix = "OpenEdge:Procedures:";

    //private readonly IDistributedCache cache;
    private readonly ILogger<OpenEdge> logger;
    private readonly IProxyInterface proxyInterface;

    public OpenEdge(/*IDistributedCache cache, */ILogger<OpenEdge> logger, IProxyInterface proxyInterface)
    {
      //this.cache = cache;
      this.logger = logger;
      this.proxyInterface = proxyInterface;
    }

    public async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
#if DEBUG
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
#endif
#if false
      if (request.Cache < 1)
      {
#endif
        logger.LogInformation("Cache result {Found} for {Procedure}", "BYPASS", request.Procedure);
        return GetProcedureResponseBytes(await GetProcedureResponse(request, cancellationToken).ConfigureAwait(false));
#if false
    }
      else
      {
        byte[] rawData = null;
        string cacheName = GetCachedKey(request.Procedure, parameterHash);
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
#endif
    }
    public async Task<ProcedureResponse> GetProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
#if DEBUG
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
#endif
#if false
      if (request.Cache < 1)
      {
#endif
      logger.LogInformation("Cache result {Found} for {Procedure}", "BYPASS", request.Procedure);
        return await GetProcedureResponse(request, cancellationToken)
          .ConfigureAwait(false);
#if false
    }
      else
      {
        string cacheName = GetCachedKey(request.Procedure, parameterHash);
        logger.LogDebug("Attempting to fetch cached response for {Procedure}", request.Procedure);
        byte[] rawData = await cache.GetAsync(cacheName, cancellationToken)
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
#endif
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
        Dictionary<string, object> outputsDictionary = GetOutputParameters(request.Parameters, parameters);

        stopwatch.Stop();
        logger.LogInformation("Execution time for {Procedure} was {ExecutionTime}", request.Procedure, stopwatch.ElapsedMilliseconds);

        if (outputsDictionary.All(x => x.Value != null))
        {
          ProcedureResponse response = new ProcedureResponse
          {
            Status = 200,
            Procedure = request.Procedure,
            OriginTime = stopwatch.ElapsedMilliseconds
          };
          if (outputsDictionary.Count == 1)
          {
            var result = outputsDictionary.First();
            if (result.Key == string.Empty)
            {
              response.Result = result.Value;
              return response;
            }
          }

          response.Result = outputsDictionary;
          return response;
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

    private Dictionary<string, object> GetOutputParameters(Parameter[] requestParameters, ParameterSet parameters)
    {
      IEnumerable<Parameter> result = requestParameters.Where(x => x.Output);
      if (result.Count() == 1)
      {
        var parameter = result.First();
        if (!parameter.HasLabel)
        {
          return new Dictionary<string, object>
          {
            { string.Empty, ExtractAndParseValue(parameter, parameters) }
          };
        }
      }

      return result.ToDictionary(x => x.ResponseLabel, x => ExtractAndParseValue(x, parameters));
    }

    private static byte[] GetProcedureResponseBytes(ProcedureResponse response)
      => JsonSerializer.SerializeToUtf8Bytes(response, serializerOptions);

    private static ProcedureResponse GetProcedureFromBytes(byte[] bytes)
      => JsonSerializer.Deserialize<ProcedureResponse>(bytes, serializerOptions);

    private static string GetCachedKey(string requestProcedure, string parameterHash)
    {
      if (!string.IsNullOrEmpty(parameterHash))
      {
        return $"{CachePathPrefix}{requestProcedure}:{parameterHash}";
      }

      return $"{CachePathPrefix}{requestProcedure}";
    }

    private static object ExtractAndParseValue(Parameter parameter, ParameterSet parameterSet)
    {
      // We have to get it out of the parameter set because Progress... Grrrrr....
      object value = parameterSet.getOutputParameter(parameter.Position);
      if (value is Progress.Open4GL.Memptr pointer)
      {
        if (parameter.Type == ParameterType.JSON)
        {
          JsonDocument result = JsonDocument.Parse(pointer.Bytes);
          if (result.RootElement.ValueKind == JsonValueKind.Array
             && result.RootElement.GetArrayLength() == 1)
          {
            return result.RootElement.EnumerateArray()
                .First();
          }

          return result;
        }

        return pointer.Bytes;
      }
      else if (value is string toParse && parameter.Type == ParameterType.LongChar)
      {
        return JsonDocument.Parse(toParse);
      }

      return value;
    }
  }
}