using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public interface IProcedureParser
  {
    /// <summary>
    /// Get a parsed result with the provided resultstring from OE
    /// </summary>
    /// <param name="returnValue">The OpenEdge return value to parse</param>
    /// <returns>Null if an invalid value was given, or the parsed result if succeeded.</returns>
    ProcedureResult GetProcedureResult(string responseString);
    /// <summary>
    /// Generate an error response from the provided ProcedureResponse and the ProcedureResult data
    /// </summary>
    /// <param name="response">Normal procedure response retrieved from OE</param>
    /// <param name="result">ProcedureResult parsed from the response value string</param>
    /// <returns>A parsed error response</returns>
    ProcedureErrorResponse GetErrorResponse(ProcedureResponse response, ProcedureResult result);
    /// <summary>
    /// Generate an error response from the provided ProcedureResponse and the ProcedureResult data
    /// </summary>
    /// <param name="status">StatusCode to return</param>
    /// <param name="procedure">Executed procedure name</param>
    /// <param name="originTime">Time taken to execute the procedure</param>
    /// <param name="result">ProcedureResult parsed from the response value string</param>
    /// <returns>A parsed error response</returns>
    ProcedureErrorResponse GetErrorResponse(int status, string procedure, long originTime, ProcedureResult result);
    /// <summary>
    /// Parse the procedure response to a byte array
    /// </summary>
    /// <param name="response">Response object to be parsed</param>
    /// <returns></returns>
    byte[] GetProcedureResponseBytes(ProcedureResponse response);
  }
}
