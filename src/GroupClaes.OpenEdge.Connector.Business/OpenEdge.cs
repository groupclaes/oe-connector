using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
  internal class OpenEdge : IOpenEdge
  {
    private const string CachePathPrefix = "OpenEdge:Procedures:";

    //private readonly IDistributedCache cache;
    protected readonly ILogger<OpenEdge> logger;
    protected readonly IParameterService parameterService;
    protected readonly IProcedureParser procedureParser;
    protected readonly IProxyProvider proxyProvider;

    public OpenEdge(/*IDistributedCache cache, */ILogger<OpenEdge> logger, IProxyProvider proxyProvider,
      IParameterService parameterService, IProcedureParser procedureParser)
    {
      //this.cache = cache;
      this.logger = logger;
      this.parameterService = parameterService;
      this.procedureParser = procedureParser;
      this.proxyProvider = proxyProvider;
    }

    public virtual async Task<byte[]> ExecuteProcedureWithTimeoutAsync(ProcedureRequest request,
      string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
      try
      {
        if (request.Timeout > 0)
        {
          try
          {
            Task<byte[]> procedureTask = ExecuteProcedureAsync(request, parameterHash, isTest,
              cancellationToken);

            return await procedureTask.WaitAsync(
              TimeSpan.FromMilliseconds(
                request.Timeout > Constants.TimeoutMaxLength
                  ? Constants.TimeoutMaxLength : request.Timeout),
              cancellationToken);
          }
          catch (Exception ex)
            when (ex is TaskCanceledException || ex is TimeoutException)
          {
            logger.LogError(ex,
              "A task has been cancelled, or the request has timed out executing procedure {Procedure}",
              request.Procedure);
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
    public virtual async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null,
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
        return procedureParser.GetProcedureResponseBytes(response);
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

    private Task<ProcedureResponse> GetProcedureResponse(ProcedureRequest request, CancellationToken cancellationToken)
    {
      return Task.Run(async () =>
      {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        ParameterSet parameters = parameterService.GenerateParameterSet(request.Parameters);

        cancellationToken.ThrowIfCancellationRequested();
        await ExecuteProcedureOnCorrectProxyInterface(request, parameters, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        Dictionary<string, object> outputsDictionary = parameterService.GetOutputParameters(request.Parameters, parameters);

        stopwatch.Stop();
        logger.LogInformation("Execution time for {Procedure} was {ExecutionTime}",
          request.Procedure, stopwatch.ElapsedMilliseconds);

        ProcedureResponse response = new ProcedureResponse
        {
          Status = outputsDictionary.All(x => x.Value != null)
            ? 200 : 204,
          Procedure = request.Procedure,
          OriginTime = stopwatch.ElapsedMilliseconds,
          Result = parameterService.GetParsedOutputs(outputsDictionary)
        };

        if (parameters.ProcedureReturnValue != null
          && parameters.ProcedureReturnValue is string returnValue
          && !string.IsNullOrWhiteSpace(returnValue))
        {
          ProcedureResult procedureResult = procedureParser.GetProcedureResult(returnValue);
          if (procedureResult == null)
          {
            logger.LogError("Invalid ProcedureReturnValue provided: {ProcedureReturnValue}", returnValue);
          }
          else
          {
            return procedureParser.GetErrorResponse(response, procedureResult);
          }
        }

        return response;
      }, cancellationToken);
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
            await Task.Delay(10, cancellationToken);
          }
        }
      }
      cancellationToken.ThrowIfCancellationRequested();
    }
  }
}