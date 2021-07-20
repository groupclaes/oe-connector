using System.Collections.Generic;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;

namespace GroupClaes.OpenEdge.Connector.Client
{
  public interface IOpenEdgeClient
  {
    /// <summary>
    /// Execute a procedure without expecting a response
    /// </summary>
    /// <param name="procedureName">Procedure name identifier</param>
    /// <param name="parameters">Procedure parameters</param>
    /// <returns></returns>
    Task ExecuteProcedure(string procedureName, IEnumerable<Parameter> parameters);
    /// <summary>
    /// Execute a prodcedure and expect a response
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<ProcedureResponse> GetProcedure(string procedureName, IEnumerable<Parameter> parameters);
  }
}