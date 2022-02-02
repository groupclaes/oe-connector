using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Progress.Open4GL.DynamicAPI;
using System.Collections.Generic;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public interface IParameterService
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="hasRedacted"></param>
    /// <param name="parameterHash"></param>
    /// <returns></returns>
    Parameter[] GetFilteredParameters(IEnumerable<Parameter> requestParameters,
      out bool hasRedacted, out string parameterHash);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="parameterSet"></param>
    /// <returns></returns>
    object ExtractAndParseValue(Parameter parameter, ParameterSet parameterSet);
    /// <summary>
    /// Generate a ParameterSet for OpenEdge request
    /// </summary>
    /// <param name="parameters">Input parameters to be used in generation</param>
    /// <returns>A parameterset instance</returns>
    ParameterSet GenerateParameterSet(IEnumerable<Parameter> parameters);
    /// <summary>
    /// Extract all procedure responses from the ouput parameters and label them if applicable.
    /// </summary>
    /// <param name="requestParameters">List of requested parameters.</param>
    /// <param name="parameters">Parameterset containing the procedure response values</param>
    /// <returns>A dictionary of the output values, keyed based on the label or fallback of the position</returns>s
    Dictionary<string, object> GetOutputParameters(IEnumerable<Parameter> requestParameters, ParameterSet parameters);
    /// <summary>
    /// Get the parsed outputs to a single result if no keys are set and only one entry exists,
    /// otherwise return the entire dictionary.
    /// </summary>
    /// <param name="outputsDictionary">Dictionary of outputs mapped by a key to validate</param>
    /// <returns>Either the single result or the inserted dictionary</returns>
    object GetParsedOutputs(Dictionary<string, object> outputsDictionary);
  }
}
