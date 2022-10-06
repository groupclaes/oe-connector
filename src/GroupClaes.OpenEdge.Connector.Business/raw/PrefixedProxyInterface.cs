using GroupClaes.OpenEdge.Connector.Business.Raw.Internal;
using Microsoft.Extensions.Logging;
using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Proxy;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal sealed class PrefixedProxyInterface : ProxyInterface
  {
    private readonly string prefixPath;
    public PrefixedProxyInterface(ILogger<PrefixedProxyInterface> logger, ProxyConnection connection, string prefixPath) : base(logger, connection)
    {
      if (!string.IsNullOrWhiteSpace(prefixPath))
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
    {
      if (prefixPath == null || procedure.StartsWith(prefixPath))
      {
        logger.LogDebug("No prefix configured, or the prefix is already prepended using {Procedure} with {Prefix}", procedure, prefixPath);
        return procedure;
      }
      else
      {
        logger.LogDebug("Prefixing procedure {Procedure} with path prefix {Prefix}", procedure, prefixPath);
        return $"{prefixPath}{procedure}";
      }
    }
  }
}
