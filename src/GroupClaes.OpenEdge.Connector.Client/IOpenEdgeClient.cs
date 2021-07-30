using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;

namespace GroupClaes.OpenEdge.Connector.Client
{
  public interface IOpenEdgeClient
  {
    /// <summary>
    /// Authenticate against the open edge connector
    /// </summary>
    /// <param name="application">Microservice application identifier</param>
    /// <param name="test">Test environment flag</param>
    /// <returns></returns>
    Task Authenticate(string application, bool test);
    /// <summary>
    /// Execute a procedure without expecting a response
    /// </summary>
    /// <param name="procedureName">Procedure name identifier</param>
    /// <param name="parameters">Procedure parameters</param>
    /// <returns></returns>
    Task ExecuteProcedure(string procedureName, int cacheTime, IEnumerable<Parameter> parameters);
    /// <summary>
    /// Execute a prodcedure and expect a response
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<ProcedureResponse> GetProcedure(string procedureName, int cacheTime, IEnumerable<Parameter> parameters);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    void RegisterResponseHandler(string procedureName, Action<ProcedureResponse> action);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    void RegisterResponseHandler(Action<ProcedureResponse> action);
    void UnregisterResponseHandler(string procedureName, Action<ProcedureResponse> action);
    void UnregisterResponseHandler(Action<ProcedureResponse> action);
  }
}