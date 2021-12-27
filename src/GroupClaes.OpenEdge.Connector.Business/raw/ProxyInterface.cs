using Progress.Open4GL;
using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Exceptions;
using Progress.Open4GL.Proxy;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal class ProxyInterface : AppObject, IProxyInterface
  {
    private new const int ProxyGenVersion = 1;
    private readonly Connection connection;

    public ProxyInterface(Connection connection)
    {
      this.connection = connection;

      if (RunTimeProperties.DynamicApiVersion != 5)
      {
        throw new Open4GLException(base.WrongProxyVer, null);
      }

      if (string.IsNullOrEmpty(connection.Url))
      {
        connection.Url = "ProxyRAW";
      }

      initAppObject("ProxyRAW", connection, RunTimeProperties.tracer, null, ProxyGenVersion);
    }

    public RqContext RunProcedure(string procName, ParameterSet params_Renamed) =>
            base.runProcedure(procName, params_Renamed);

    public RqContext RunProcedure(string procName, ParameterSet params_Renamed, MetaSchema schema) =>
        base.runProcedure(procName, params_Renamed, schema);

    public RqContext RunProcedure(string requestID, string procName, ParameterSet params_Renamed, MetaSchema schema) =>
        base.runProcedure(requestID, procName, params_Renamed, schema);

    public new virtual void Dispose()
    {
      base.Dispose();
      this.connection.Dispose();
    }
  }
}
