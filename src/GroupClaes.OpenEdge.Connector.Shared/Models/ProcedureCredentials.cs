using System.Text.Json.Serialization;

namespace GroupClaes.OpenEdge.Connector.Shared.Models
{
  public class ProcedureCredentials
  {
    /// <summary>
    /// Application identifier in configuration
    /// </summary>
    [JsonPropertyName("app")]
    public string AppServer { get; set; }
    /// <summary>
    /// Username to override
    /// </summary>
    [JsonPropertyName("user")]
    public string Username { get; set; }
    /// <summary>
    /// Password to override
    /// </summary>
    [JsonPropertyName("pwd")]
    public string Password { get; set; }
  }
}
