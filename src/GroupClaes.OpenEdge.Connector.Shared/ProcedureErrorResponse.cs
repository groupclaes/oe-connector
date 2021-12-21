using System;
using System.Collections.Generic;
using System.Text;

namespace GroupClaes.OpenEdge.Connector.Shared
{
  public class ProcedureErrorResponse : ProcedureResponse
  {
    public string Title { get; set; }
    public string Description { get; set; }
  }
}
