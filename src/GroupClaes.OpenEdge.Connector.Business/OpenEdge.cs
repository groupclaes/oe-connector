using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Business.Exceptions;
using GroupClaes.OpenEdge.Connector.Business.Raw;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.Extensions.Logging;
using Progress.Open4GL.DynamicAPI;
using static Progress.Open4GL.DynamicAPI.SessionPool;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public class OpenEdge : IOpenEdge
  {
    private static readonly JsonSerializerOptions SerializerOptions
      = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private const string CachePathPrefix = "OpenEdge:Procedures:"; 

    //private readonly IDistributedCache cache;
    private readonly ILogger<OpenEdge> logger;
    private readonly IProxyProvider proxyProvider;
    private readonly IChecksumService checksumService;

    public OpenEdge(/*IDistributedCache cache, */ILogger<OpenEdge> logger, IChecksumService checksumService,
      IProxyProvider proxyProvider)
    {
      //this.cache = cache;
      this.logger = logger;
      this.proxyProvider = proxyProvider;
      this.checksumService = checksumService;
    }

    public async Task<byte[]> ExecuteProcedureWithTimeoutAsync(ProcedureRequest request,
      string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
      try
      {
        if (request.Timeout > 0)
        {
          try
          {
            Task<byte[]> procedureTask = ExecuteProcedureAsync(request, parameterHash, isTest, cancellationToken);

            return await procedureTask.WaitAsync(
              TimeSpan.FromMilliseconds(
                request.Timeout > Constants.TimeoutMaxLength
                  ? Constants.TimeoutMaxLength : request.Timeout),
              cancellationToken);
          }
          catch (Exception ex)
            when (ex is TaskCanceledException || ex is TimeoutException)
          {
            logger.LogError(ex, "A task has been cancelled, or the request has timed out executing procedure {Procedure}", request.Procedure);
            throw new OpenEdgeTimeoutException();
          }
        }
        else
        {
          return await ExecuteProcedureAsync(request, parameterHash, isTest, cancellationToken);
        }
      }
      catch (NoAvailableSessionsException ex)
      {
        logger.LogError(ex, "No available session, the connection was refused by open edge when executing procedure {Procedure}", request.Procedure);
        throw new OpenEdgeRefusedException(ex);
      }
      catch (Exception ex)
        when(!(ex is OpenEdgeTimeoutException))
      {
        logger.LogError(ex, "An unhandled exception occurred when executing procedure {Procedure}", request.Procedure);
        throw;
      }
    }
    public async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null,
      bool isTest = false, CancellationToken cancellationToken = default)
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
        ProcedureResponse response = await GetProcedureResponse(request, cancellationToken);
        return GetJsonBytes(response);
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
    public async Task<ProcedureResponse> GetProcedureAsync(ProcedureRequest request,
      string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
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
            hashBuilder.Append(request.Parameters[i].Position);
            hashBuilder.Append(request.Parameters[i].Value);
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

      parameterHash = hashBuilder.Length > 0 ? checksumService.Generate(hashBuilder) : string.Empty;

      return hasRedacted;
    }

    private Task<ProcedureResponse> GetProcedureResponse(ProcedureRequest request, CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        ParameterSet parameters = GenerateParameterSet(request.Parameters);

        cancellationToken.ThrowIfCancellationRequested();
        await ExecuteProcedureOnCorrectProxyInterface(request, parameters, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        Dictionary<string, object> outputsDictionary = GetOutputParameters(request.Parameters, parameters);

        stopwatch.Stop();
        logger.LogInformation("Execution time for {Procedure} was {ExecutionTime}",
          request.Procedure, stopwatch.ElapsedMilliseconds);

        ProcedureResponse response = new ProcedureResponse
        {
          Status = outputsDictionary.All(x => x.Value != null)
            ? 200 : 204,
          Procedure = request.Procedure,
          OriginTime = stopwatch.ElapsedMilliseconds,
          Result = GetParsedOutputs(outputsDictionary)
        };

        if (parameters.ProcedureReturnValue != null
          && parameters.ProcedureReturnValue is string returnValue
          && !string.IsNullOrWhiteSpace(returnValue))
        {
          ProcedureResult procedureResult = GetProcedureResult(returnValue);
          if (procedureResult == null)
          {
            logger.LogError("Invalid ProcedureReturnValue provided: {ProcedureReturnValue}", returnValue);
          }
          else
          {
            return GenerateErrorResponse(response, procedureResult);
          }
        }

        return response;
      }, cancellationToken);
    }

    internal static object ExtractAndParseValue(Parameter parameter, ParameterSet parameterSet)
    {
      // We have to get it out of the parameter set because Progress... Grrrrr....
      object value = parameterSet.getOutputParameter(parameter.Position);
      if (value is Progress.Open4GL.Memptr pointer)
      {
        if (parameter.Type == ParameterType.JSON)
        {
          if (pointer.Bytes != null && pointer.Bytes.Any())
          {
            JsonDocument result = JsonDocument.Parse(pointer.Bytes);
            // Check if the element is an array, array isn't forced and length is 1
            if (result.RootElement.ValueKind == JsonValueKind.Array
               && !parameter.ForceArray && result.RootElement.GetArrayLength() == 1)
            {
              return result.RootElement.EnumerateArray()
                  .First();
            }

            return result;
          }
          else
          {
            return null;
          }
        }

        return pointer.Bytes;
      }
      else if (value is string toParse && parameter.Type == ParameterType.LongChar)
      {
        return JsonDocument.Parse(toParse);
      }

      return value;
    }


    /// <summary>
    /// Generate a ParameterSet from the provided parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    internal static ParameterSet GenerateParameterSet(IEnumerable<Parameter> parameters)
    {
      ParameterSet parameterSet = new ParameterSet(parameters.Count());
      foreach (Parameter parameter in parameters)
      {
        if (parameter.Value != null)
        {
          int inputOutputType = parameter.Output ? ParameterSet.OUTPUT : ParameterSet.INPUT;
          JsonElement value = (JsonElement)parameter.Value;
          if (!parameter.Output && parameter.Type == ParameterType.JSON)
          {
            byte[] valueArray = GetJsonBytes(parameter.Value);
            parameterSet.setParameter(parameter.Position, new Progress.Open4GL.Memptr(valueArray), inputOutputType,
              GetParameterSetType(parameter), false, 0, null);
          }
          else if (value.ValueKind == JsonValueKind.Array)
          {
            object[] values = (object[])ExtractValueFromJsonElement(value);
            parameterSet.setParameter(parameter.Position, values, inputOutputType,
                GetParameterSetType(parameter), (values.Length > 0), values.Length, null);
          }
          else
          {
            parameterSet.setParameter(parameter.Position, ExtractValueFromJsonElement(value),
                inputOutputType, GetParameterSetType(parameter), false, 0, null);
          }
        }
        else
        {
          parameterSet.setParameter(parameter.Position, null, ParameterSet.OUTPUT,
              GetParameterSetType(parameter), false, 0, null);
        }
      }

      return parameterSet;
    }



    /// <summary>
    /// Extract a value from a JsonElement
    /// </summary>
    /// <param name="value">JSON Element to retrieve from</param>
    internal static object ExtractValueFromJsonElement(JsonElement value)
    {
      switch (value.ValueKind)
      {
        case JsonValueKind.Array:
          object[] values = value.EnumerateArray()
            .Select(x => ExtractValueFromJsonElement(x))
            .ToArray();
          return values;
        case JsonValueKind.True:
          return true;
        case JsonValueKind.False:
          return false;
        case JsonValueKind.Number:
          return value.GetInt32();
        case JsonValueKind.Object:
          Dictionary<string, object> objectProperties = value.EnumerateObject()
              .ToDictionary(x => x.Name, x => ExtractValueFromJsonElement(x.Value));
          return objectProperties;
        default:
          return value.GetString();
      }
    }
    /// <summary>
    /// Generate an error response from the provided ProcedureResponse and the ProcedureREsult data
    /// </summary>
    /// <param name="response">Normal procedure response retrieved from OE</param>
    /// <param name="result">ProcedureResult parsed from the response value string</param>
    /// <returns>A parsed error response</returns>
    internal static ProcedureErrorResponse GenerateErrorResponse(ProcedureResponse response, ProcedureResult result)
    {
      ProcedureErrorResponse errorResponse = new ProcedureErrorResponse
      {
        Status = result.StatusCode,
        Description = result.Description,
        Title = result.Title,

        Procedure = response.Procedure,
        LastModified = response.LastModified,
        OriginTime = response.OriginTime,
        Result = response.Result
      };

      return errorResponse;
    }
    /// <summary>
    /// Get the cache key based upon the procedure and the parameter hash
    /// </summary>
    /// <param name="requestProcedure">Procedure name to include in the key</param>
    /// <param name="parameterHash">Parameter hash to differentiate requested datas</param>
    /// <returns>The key to be used to access from the cache</returns>
    internal static string GetCachedKey(string requestProcedure, string parameterHash)
    {
      if (!string.IsNullOrEmpty(parameterHash))
      {
        return $"{CachePathPrefix}{requestProcedure}:{parameterHash}";
      }

      return $"{CachePathPrefix}{requestProcedure}";
    }
    /// <summary>
    /// Serialize an object to a json byte array
    /// </summary>
    /// <param name="value">Input object to parse to json</param>
    internal static byte[] GetJsonBytes(object value)
      => JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
    /// <summary>
    /// Extract all procedure responses from the ouput parameters and label them if applicable.
    /// </summary>
    /// <param name="requestParameters">List of requested parameters.</param>
    /// <param name="parameters">Parameterset containing the procedure response values</param>
    /// <returns>A dictionary of the output values, keyed based on the label or fallback of the position</returns>
    internal static Dictionary<string, object> GetOutputParameters(Parameter[] requestParameters, ParameterSet parameters)
    {
      IEnumerable<Parameter> result = requestParameters.Where(x => x.Output);
      if (result.Count() == 1)
      {
        Parameter parameter = result.First();
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
    /// <summary>
    /// Get a Progress compliant parametertype integer.
    /// </summary>
    /// <param name="type">Parametertype to convert</param>
    internal static int GetParameterSetType(Parameter parameter)
    {
      if (parameter.Type == ParameterType.Undefined)
      {
        if (parameter.Output)
        {
          // Json is the default output parameter
          parameter.Type = ParameterType.JSON;
          return (int)ParameterType.MemPointer;
        }
        else
        {
          // Default parameter
          parameter.Type = ParameterType.String;
        }
      }
      else if (parameter.Type == ParameterType.JSON)
      {
        // JSON should become a mempointer
        return (int)ParameterType.MemPointer;
      }

      return (int)parameter.Type;
    }
    /// <summary>
    /// Get the parsed outputs to a single result if no keys are set and only one entry exists,
    /// otherwise return the entire dictionary.
    /// </summary>
    /// <param name="outputsDictionary">Dictionary of outputs mapped by a key to validate</param>
    /// <returns>Either the single result or the inserted dictionary</returns>
    internal static object GetParsedOutputs(Dictionary<string, object> outputsDictionary)
    {
      if (outputsDictionary.Count == 1)
      {
        var result = outputsDictionary.First();
        if (result.Key == string.Empty)
        {
          return result.Value;
        }
      }

      return outputsDictionary;
    }
    /// <summary>
    /// Parse a ProcedureResponse byte array to an object instance.
    /// </summary>
    /// <param name="bytes">Bytes array of json data to be parsed</param>
    /// <returns>The serialized version of the byte array</returns>
    internal static ProcedureResponse GetProcedureFromBytes(byte[] bytes)
      => JsonSerializer.Deserialize<ProcedureResponse>(bytes, SerializerOptions);
    /// <summary>
    /// Get a parsed result with the provided resultstring from OE
    /// </summary>
    /// <param name="returnValue">The OpenEdge return value to parse</param>
    /// <returns>Null if an invalid value was given, or the parsed result if succeeded.</returns>
    internal static ProcedureResult GetProcedureResult(string returnValue)
    {
      string[] returnCode = returnValue.Split(new string[] { "::" }, 3, StringSplitOptions.None);
      if (returnCode.Length > 1)
      {
        ProcedureResult result = new ProcedureResult();
        if (Regexes.HttpStatusCode.IsMatch(returnCode[0]))
        {
          result.StatusCode = int.Parse(returnCode[0]);
          result.Title = returnCode[1];

          if (returnCode.Length == 3)
          {
            result.Description = returnCode[2];
          }

          return result;
        }
      }

      return null;
    }

    private async Task ExecuteProcedureOnCorrectProxyInterface(ProcedureRequest request, ParameterSet parameters,
      CancellationToken cancellationToken = default)
    {
      IProxyInterface proxyInterface;
      if (request.Credentials != null)
      {
        proxyInterface = proxyProvider.CreateProxyInstance(
          request.Credentials.AppServer ?? Constants.DefaultOpenEdgeEndpoint,
          request.Credentials.Username,
          request.Credentials.Password,
          null, null);
      }
      else
      {
        proxyInterface = proxyProvider.CreateProxyInstance();
      }

      using (proxyInterface)
      {
        using (Task procedureTask = Task.Run(() => proxyInterface.RunProcedure(request.Procedure, parameters)))
        {
          // Check if the task has been cancelled.
          while (!procedureTask.IsCompleted)
          {
            await Task.Delay(1, cancellationToken);
          }
        }
      }
      cancellationToken.ThrowIfCancellationRequested();
    }
  }
}