using System;

namespace GroupClaes.OpenEdge.Connector.Business.Exceptions
{
  public class OpenEdgeException : Exception
  {
    protected OpenEdgeException() {}
    protected OpenEdgeException(string message) : base(message) {}
    protected OpenEdgeException(string message, Exception nestedException) : base(message, nestedException) {}
    protected OpenEdgeException(Exception nestedException) : base(null, nestedException) {}
  }
}
