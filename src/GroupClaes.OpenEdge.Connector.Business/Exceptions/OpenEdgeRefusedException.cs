using System;
using GroupClaes.OpenEdge.Connector.Shared.Models;

namespace GroupClaes.OpenEdge.Connector.Business.Exceptions
{
  public class OpenEdgeRefusedException : OpenEdgeException
  {
    public ProcedureResult Result { get; init; }
    public OpenEdgeRefusedException(Exception nestedException)
      : base(nestedException.Message, nestedException)
    {
    }
    public OpenEdgeRefusedException(ProcedureResult result, Exception nestedException)
      : base(nestedException.Message, nestedException)
    {
      this.Result = result;
    }
  }
}
