using Microsoft.Extensions.Logging;
using Progress.Open4GL;
using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Exceptions;
using Progress.Open4GL.Proxy;
using System;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  internal class ProxyInterface : AppObject, IProxyInterface
  {
    protected readonly ILogger<ProxyInterface> logger;

    private new const int ProxyGenVersion = 1;
    private const int CurrentDynamicApiVersion = 5;
    private readonly Connection connection;

    public ProxyInterface(ILogger<ProxyInterface> logger, Connection connection)
    {
      this.connection = connection;
      this.logger = logger;

      if (RunTimeProperties.DynamicApiVersion != CurrentDynamicApiVersion)
      {
        throw new Open4GLException(base.WrongProxyVer, null);
      }

      if (string.IsNullOrEmpty(connection.Url))
      {
        logger.LogWarning("Provided connection Url is empty, defaulting to ProxyRAW");
        connection.Url = "ProxyRAW";
      }

      initAppObject("ProxyRAW", connection, RunTimeProperties.tracer, null, ProxyGenVersion);
      connection.ReleaseConnection();
    }

    public virtual RqContext RunProcedure(string procName, ParameterSet params_Renamed)
      => base.runProcedure(procName, params_Renamed);

    public virtual RqContext RunProcedure(string procName, ParameterSet params_Renamed, MetaSchema schema)
      => base.runProcedure(procName, params_Renamed, schema);

    public virtual RqContext RunProcedure(string requestID, string procName, ParameterSet params_Renamed, MetaSchema schema)
      => base.runProcedure(requestID, procName, params_Renamed, schema);

    public new void Dispose()
    {
      if (!disposed)
      {
        disposed = true;

        logger.LogTrace("Disposing ProxyInterface");
        connection.ReleaseConnection();
        base.Dispose();
        logger.LogTrace("Disposed ProxyInterface");
        logger.LogTrace("Disposing Connection");
        connection.Dispose();
        logger.LogTrace("Disposed Connection");
        GC.WaitForPendingFinalizers();
      }
    }
  }
}
