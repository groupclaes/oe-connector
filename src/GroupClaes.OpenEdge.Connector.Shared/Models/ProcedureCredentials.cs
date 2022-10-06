using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GroupClaes.OpenEdge.Connector.Shared.Models
{
  public class ProcedureCredentials
  {
    /// <summary>
    /// Application identifier in configuration
    /// </summary>
    [JsonPropertyName("app")]
    [MaxLength(24, ErrorMessage = "Appserver name max length is 24 characters")]
    [RegularExpression(Regexes.AppServerString,
         ErrorMessage = "Appserver name must be alphanumberic and may contain dashes and underscores")]
    public string AppServer { get; set; }
    /// <summary>
    /// Username to override
    /// </summary>
    [JsonPropertyName("user")]
    [MaxLength(32, ErrorMessage = "Username max length is 32 characters")]
    [RegularExpression(Regexes.UsernameString,
         ErrorMessage = "Username must be alphanumberic and may contain dashes and underscores")]
    public string Username { get; set; }
    /// <summary>
    /// Password to override
    /// </summary>
    [JsonPropertyName("pwd")]
    [MaxLength(32, ErrorMessage = "Password max length is 32 characters")]
    [RegularExpression(Regexes.PasswordString,
         ErrorMessage = "Password must be alphanumberic and may contain: -_@$!%*#?&")]
    public string Password { get; set; }
    /// <summary>
    /// Internal property?
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public string ProcedurePrefix { get; set; }
    /// <summary>
    /// Internal property?
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public string Endpoint { get; set; }
    /// <summary>
    /// Internal property?
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public string AppId { get; set; }
  }
}
