using System;
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
    public object Result { get; set; }
    /// <summary>
    /// The timestamp of the retrieved result
    /// </summary>
    [JsonPropertyName("lastMod")]
    public DateTime? LastModified { get; set; }
    /// <summary>
    /// Time taken to execute and receive the procedure response
    /// </summary>
    /// <remark>If cached, the original elapsed time will still be shown</remark>
    [JsonPropertyName("origTime")]
    public long OriginTime { get; set; }
  }
}
