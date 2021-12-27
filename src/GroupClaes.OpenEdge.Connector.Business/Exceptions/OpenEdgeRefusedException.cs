using System;

namespace GroupClaes.OpenEdge.Connector.Business.Exceptions
{
  public class OpenEdgeRefusedException : OpenEdgeException
  {
    public OpenEdgeRefusedException(Exception nestedException)
      : base(nestedException.Message, nestedException) {}
  }
}
