using Progress.Open4GL;
using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Exceptions;
using Progress.Open4GL.Proxy;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal class ProxyInterface : AppObject, IProxyInterface
  {
    private new const int ProxyGenVersion = 1;
    private const int CurrentDynamicApiVersion = 5;
    private readonly Connection connection;

    public ProxyInterface(Connection connection)
    {
      this.connection = connection;

      if (RunTimeProperties.DynamicApiVersion != CurrentDynamicApiVersion)
      {
        throw new Open4GLException(base.WrongProxyVer, null);
      }

      if (string.IsNullOrEmpty(connection.Url))
      {
        connection.Url = "ProxyRAW";
      }

      initAppObject("ProxyRAW", connection, RunTimeProperties.tracer, null, ProxyGenVersion);
    }

    public virtual RqContext RunProcedure(string procName, ParameterSet params_Renamed)
      => base.runProcedure(procName, params_Renamed);

    public virtual RqContext RunProcedure(string procName, ParameterSet params_Renamed, MetaSchema schema)
      => base.runProcedure(procName, params_Renamed, schema);

    public virtual RqContext RunProcedure(string requestID, string procName, ParameterSet params_Renamed, MetaSchema schema)
      => base.runProcedure(requestID, procName, params_Renamed, schema);

    public new virtual void Dispose()
    {
      if (!disposed)
      {
        this.connection.Dispose();
        base.Dispose();
      }
    }
  }
}
