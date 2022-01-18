using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Proxy;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal sealed class PrefixedProxyInterface : ProxyInterface
  {
    private readonly string prefixPath;
    public PrefixedProxyInterface(Connection connection, string prefixPath) : base(connection)
    {
      if (prefixPath != null)
      {
        if (prefixPath.EndsWith("/"))
        {
          this.prefixPath = prefixPath;
        }
        else
        {
          this.prefixPath = prefixPath + "/";
        }
      }
    }


    public override RqContext RunProcedure(string procName, ParameterSet params_Renamed) 
      => base.runProcedure(GetPrefixPath(procName), params_Renamed);

    public override RqContext RunProcedure(string procName, ParameterSet params_Renamed, MetaSchema schema) 
      => base.runProcedure(GetPrefixPath(procName), params_Renamed, schema);

    public override RqContext RunProcedure(string requestID, string procName, ParameterSet params_Renamed, MetaSchema schema)
      => base.runProcedure(requestID, GetPrefixPath(procName), params_Renamed, schema);

    private string GetPrefixPath(string procedure)
      => procedure.StartsWith(prefixPath)
        ? procedure : $"{prefixPath}{procedure}";
  }
}
