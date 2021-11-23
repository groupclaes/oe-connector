using System.Text.Json.Serialization;
using GroupClaes.OpenEdge.Connector.Shared.Models;

namespace GroupClaes.OpenEdge.Connector.Shared
{
  public class ProcedureRequest
  {
    /// <summary>
    /// Procedure name identifier
    /// </summary>
    [JsonPropertyName("proc")]
    public string Procedure { get; set; }
    /// <summary>
    /// Parameters to append when executing the procedure
    /// </summary>
    [JsonPropertyName("parm")]
    public Parameter[] Parameters { get; set; }
    /// <summary>
    /// Time in seconds for the entry to be cached to disk.
    /// </summary>
    /// <remark>The response will only be cached if there isn't one available yet.</remark>
    public int Cache { get; set; }
    /// <summary>
    /// Time in seconds for a request to time out and a 408 to be returned.
    /// </summary>
    [JsonPropertyName("tw")]
    public int Timeout { get; set; }
  }
}
