using Progress.Open4GL;
using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Proxy;
using RAW;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
    internal class ProxyInterface : ProxyRAW, IProxyInterface
    {
        public ProxyInterface(Connection c) : base(c) {}

        public RqContext RunProcedure(string procName, ParameterSet params_Renamed) =>
                base.runProcedure(procName, params_Renamed);

        public RqContext RunProcedure(string procName, ParameterSet params_Renamed, MetaSchema schema) =>
            base.runProcedure(procName, params_Renamed, schema);

        public RqContext RunProcedure(string requestID, string procName, ParameterSet params_Renamed, MetaSchema schema) =>
            base.runProcedure(requestID, procName, params_Renamed, schema);
    }
}
