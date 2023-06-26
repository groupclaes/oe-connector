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
using Progress.Open4GL.Exceptions;
using static Progress.Open4GL.DynamicAPI.SessionPool;

namespace GroupClaes.OpenEdge.Connector.Business
{
  internal class OpenEdge : IOpenEdge
  {
    protected readonly ILogger<OpenEdge> logger;
    protected readonly IParameterService parameterService;
    protected readonly IProcedureParser procedureParser;
    protected readonly IProxyProvider proxyProvider;

    public OpenEdge(ILogger<OpenEdge> logger, IProxyProvider proxyProvider,
      IParameterService parameterService, IProcedureParser procedureParser)
    {
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
          TimeSpan timeoutSpan =  TimeSpan.FromMilliseconds(
                request.Timeout > Constants.TimeoutMaxLength
                  ? Constants.TimeoutMaxLength : request.Timeout);
          try
          {
            Task<byte[]> procedureTask = ExecuteProcedureAsync(request, parameterHash, isTest,
              cancellationToken);

            return await procedureTask.WaitAsync(
              timeoutSpan,
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
      catch (OpenEdgeRefusedException ex)
      {
        logger.LogError(ex, "No available session, the connection was refused by open edge when executing procedure {Procedure}", request.Procedure);
        throw;
      }
      catch (Exception ex)
        when(ex is not OpenEdgeException)
      {
        logger.LogError(ex, "An unhandled exception occurred when executing procedure {Procedure}", request.Procedure);
        throw;
      }
    }
    /// <summary>
    /// Execute the procedure and retrieve the raw bytes of the json parsed string
    /// </summary>
    /// <param name="request"></param>
    /// <param name="parameterHash"></param>
    /// <param name="isTest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null,
      bool isTest = false, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Cache result {Found} for {Procedure}", "BYPASS", request.Procedure);
        ProcedureResponse response = await GetProcedureResponse(request, cancellationToken);
        return procedureParser.GetProcedureResponseBytes(response);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="parameterHash"></param>
    /// <param name="isTest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ProcedureResponse> GetProcedureAsync(ProcedureRequest request,
      string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default)
    {
#if DEBUG
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
#endif
      logger.LogInformation("Cache result {Found} for {Procedure}", "BYPASS", request.Procedure);
      ProcedureResponse response = await GetProcedureResponse(request, cancellationToken)
        .ConfigureAwait(false);
#if DEBUG
      stopwatch.Stop();
      logger.LogTrace("ExecuteProcedureAsync time taken: {ElapsedTime}", stopwatch.Elapsed);
#endif

      return response;
    }

    protected Task<ProcedureResponse> GetProcedureResponse(ProcedureRequest request, CancellationToken cancellationToken)
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

    private async Task ExecuteProcedureOnCorrectProxyInterface(ProcedureRequest request, ParameterSet parameters,
      CancellationToken cancellationToken = default)
    {
      IProxyInterface proxyInterface = null;
      try
      {
        try 
        {
          proxyInterface = GetProxyInterface(request.Credentials);

          await RunProcedureOnInterface(proxyInterface, request.Procedure,
            parameters, cancellationToken);
        }
        catch (NoAvailableSessionsException ex)
        {
            ProcedureResult result = procedureParser.GetProcedureResult(
              (ex as NoAvailableSessionsException).ProcReturnString);
            if (result != null && result.StatusCode != 403)
            {
              logger.LogCritical(ex, "First execution attempt failed with reason: {Reason}, retrying with new proxy instance...", ex.ProcReturnString);
              // Retry once
              proxyInterface = GetProxyInterface(request.Credentials);

              await RunProcedureOnInterface(proxyInterface, request.Procedure,
                parameters, cancellationToken);
            }
            else
            {
              proxyProvider.CloseConnection(proxyInterface);
              throw new OpenEdgeRefusedException(result, ex);
            }
        }
      }
      
      catch (Open4GLException ex)
        when (ex is ConnectException
          || ex is SessionPoolException)
      {
        if (!string.IsNullOrWhiteSpace(ex.ProcReturnString))
        {
          ProcedureResult result = procedureParser.GetProcedureResult(
            ex.ProcReturnString);

          proxyProvider.CloseConnection(proxyInterface);
          throw new OpenEdgeRefusedException(result, ex);
        }


        proxyProvider.CloseConnection(proxyInterface);
        throw new OpenEdgeRefusedException(
          new ProcedureResult
          {
            StatusCode = 500,
            Title = "OpenEdge unavailable"
          }, ex);
      }
      finally
      {
        if (proxyInterface != null)
        {
          proxyInterface.Dispose();
        }
      }
    }

    private async Task RunProcedureOnInterface(IProxyInterface proxyInterface, string procedure,
      ParameterSet parameters, CancellationToken cancellationToken = default)
    {
      Task<RqContext> procedureTask = Task.Run(
        () => proxyInterface.RunProcedure(procedure, parameters),
        cancellationToken);

      await procedureTask.WaitAsync(cancellationToken);

      if (procedureTask.IsFaulted)
      {
        logger.LogError(procedureTask.Exception,
          "Something went wrong when executing procedure task in proxy-interface {Procedure}",
          procedure);
        throw procedureTask.Exception;
      }
      else if (procedureTask.Result != null)
      {
        procedureTask.Result.Release();
      }
    }

    private IProxyInterface GetProxyInterface(ProcedureCredentials credentials)
    {
      if (credentials == null)
      {
        return proxyProvider.CreateProxyInstance();
      }

      if (string.IsNullOrWhiteSpace(credentials.AppServer))
      {
        credentials.AppServer = Constants.DefaultOpenEdgeEndpoint;
      }

      return proxyProvider.CreateProxyInstance(credentials);
    }
  }
}