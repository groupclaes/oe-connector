using System;
using System.Collections.Generic;
using System.Text;

namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  public interface IProxyProvider
  {
    IProxyInterface CreateProxyInstance();
    IProxyInterface CreateProxyInstance(string appServer);
    IProxyInterface CreateProxyInstance(string appServer, string userId, string password);
    IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo);
    IProxyInterface CreateProxyInstance(string appServer, string userId, string password, string appServerInfo, string procedurePrefix);
  }
}
