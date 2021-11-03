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
                /*string paramString = "[";
                try {
                int nums = params_Renamed.NumParams;
                for ( int i = 1; i <= params_Renamed.NumParams; i++ ) {
                if ( params_Renamed.getParameterInOut( i ) == 2 ) {
                // is out => try get out
                paramString += $"{i}: outParam";
                } else {
                paramString += $"{i}: { params_Renamed.getParameter( i ) }";
                }
                if ( i != nums ) {
                paramString += ",";
                }
                }
                } catch ( Exception ex ) {
                iTrace.Error( _name, ex );
                }
                paramString += "]";
                // iTrace.Debug( _name, $"Prx.runProcedure('{procName}',{paramString}) instance {instanceGuid}" );*/
                base.runProcedure(procName, params_Renamed);

        public RqContext RunProcedure(string procName, ParameterSet params_Renamed, MetaSchema schema) =>
            base.runProcedure(procName, params_Renamed, schema);

        public RqContext RunProcedure(string requestID, string procName, ParameterSet params_Renamed, MetaSchema schema) =>
            base.runProcedure(requestID, procName, params_Renamed, schema);
    }
}
