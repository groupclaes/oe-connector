using System;
using System.Collections.Generic;
using System.Text;
using GroupClaes.OpenEdge.Connector.Business.Raw.Internal;
using GroupClaes.OpenEdge.Connector.Shared.Models;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  public interface IProxyProvider
  {
    bool CloseConnection(ProxyConnection connection);
    bool CloseConnection(IProxyInterface proxyInterface);

    IProxyInterface CreateProxyInstance();
    IProxyInterface CreateProxyInstance(string appServer);
    IProxyInterface CreateProxyInstance(string appServer, string userId, string password);
    IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo);
    IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo, string procedurePrefix);
    IProxyInterface CreateProxyInstance(ProcedureCredentials credentials);
  }
}
