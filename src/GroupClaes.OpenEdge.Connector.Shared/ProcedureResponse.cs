using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GroupClaes.OpenEdge.Connector.Shared
{
  public class ProcedureResponse
  {
    /// <summary>
    /// Procedure name identifier
    /// </summary>
    [JsonPropertyName("proc")]
    public string Procedure { get; set; }
    /// <summary>
    /// Procedure success statuscode
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// Resulting responses by OpenEdge with the index as key
    /// </summary>
    public Dictionary<int, object> Result { get; set; }
    /// <summary>
    /// The age of the cached content
    /// </summary>
    /// <remark>-1 means it hasn't been cached</remark>
    public int Age { get; set; }
    /// <summary>
    /// Time taken to execute and receive the procedure response
    /// </summary>
    /// <remark>If cached, the original elapsed time will still be shown</remark>
    public long ElapsedTime { get; set; }
  }
}
