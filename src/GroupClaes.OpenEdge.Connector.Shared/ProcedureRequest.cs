using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading;
using GroupClaes.OpenEdge.Connector.Shared.Models;

namespace GroupClaes.OpenEdge.Connector.Shared
{
  public class ProcedureRequest
  {
    /// <summary>
    /// Procedure name identifier
    /// </summary>
    [JsonPropertyName("proc")]
    [RegularExpression(Regexes.ProcedurePathString,
         ErrorMessage = "Procedure name must be alphanumberic and may contain dashes, " + 
            "underscores and a dot and can be prefixed by a path.")]
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
    [Range(-1, Constants.TimeoutMaxLength)]
    public int Cache { get; set; }
    /// <summary>
    /// Time in seconds for a request to time out and a 408 to be returned.
    /// </summary>
    [JsonPropertyName("tw")]
    [Range(-1, Constants.CacheMaxLength)]
    public int Timeout { get; set; }
    [JsonPropertyName("creds")]
    public ProcedureCredentials Credentials { get; set; }
  }
}
