using Microsoft.Extensions.Logging;
using Progress.Open4GL;
using Progress.Open4GL.DynamicAPI;
using Progress.Open4GL.Exceptions;
using Progress.Open4GL.Proxy; 

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

      logger.LogTrace("Available sessions {AvailableSessionCount}", this.SessionPool.availableSessions());
      initAppObject("ProxyRAW", connection, RunTimeProperties.tracer, null, ProxyGenVersion);
    }

    public virtual RqContext RunProcedure(string procName, ParameterSet params_Renamed)
      => base.runProcedure(procName, params_Renamed);

    public virtual RqContext RunProcedure(string procName, ParameterSet params_Renamed, MetaSchema schema)
      => base.runProcedure(procName, params_Renamed, schema);

    public virtual RqContext RunProcedure(string requestID, string procName, ParameterSet params_Renamed, MetaSchema schema)
      => base.runProcedure(requestID, procName, params_Renamed, schema);

    public void Dispose()
    {

    }

    protected override void Dispose(bool disposing)
    {
      if (disposed)
      {
        return;
      }

      if (disposing)
      {
        base.CancelAllRequests();

        logger.LogTrace("Disposing ProxyInterface");
        base.Dispose(disposing);
        ProxyProvider.RemoveActiveProvider();
        logger.LogCritical("Disposed ProxyInterface, Active Providers {ActiveProviders}, {AvailableSessions}, {PoolSize}",
          ProxyProvider.ActiveProviders, this.SessionPool.availableSessions(),
          this.SessionPool.size());
      }

      disposed = true;
    }
  }
}
