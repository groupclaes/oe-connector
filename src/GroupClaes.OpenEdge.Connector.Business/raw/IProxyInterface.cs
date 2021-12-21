using Progress.Open4GL.DynamicAPI;
using System;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
    public interface IProxyInterface : IDisposable
    {
        /// <summary>
        /// Run a procedure in RAW
        /// </summary>
        /// <param name="procedureName">Procedure name identifier</param>
        /// <param name="parameters">Set of parameters with inputs and outputs</param>
        /// <returns>An RqContext instance</returns>
        RqContext RunProcedure(string procedureName, ParameterSet parameters);
        /// <summary>
        /// Run a procedure in RAW
        /// </summary>
        /// <param name="procedureName">Procedure name identifier</param>
        /// <param name="parameters">Set of parameters with inputs and outputs</param>
        /// <param name="schema"></param>
        /// <returns>An RqContext instance</returns>
        RqContext RunProcedure(string procedureName, ParameterSet parameters, MetaSchema schema);
        /// <summary>
        /// Run a procedure in RAW
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="procedureName">Procedure name identifier</param>
        /// <param name="parameters">Set of parameters with inputs and outputs</param>
        /// <param name="schema"></param>
        /// <returns>An RqContext instance</returns>
        RqContext RunProcedure(string requestId, string procedureName, ParameterSet parameters, MetaSchema schema);
    }
}
