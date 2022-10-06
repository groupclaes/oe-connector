using System.Collections.Concurrent;
using Progress.Open4GL.Proxy;

namespace GroupClaes.OpenEdge.Connector.Business.Raw.Internal
{
    public class ProxyConnection
    {
        public Connection Connection { get; init; }
        public string Hash { get; init; }
        public ConcurrentBag<IProxyInterface> ProxyInterfaces { get; init; }
            = new ConcurrentBag<IProxyInterface>();

        public ProxyConnection(string hash, Connection connection)
        {
            this.Connection = connection;
            this.Hash = hash;
        }
    }
}