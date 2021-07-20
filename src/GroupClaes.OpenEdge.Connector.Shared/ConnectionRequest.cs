
using System.Text.Json.Serialization;

namespace GroupClaes.OpenEdge.Connector.Shared
{
  public class ConnectionRequest
  {
    /// <summary>
    /// Application identifier 
    /// </summary>
    /// <value></value>
    [JsonPropertyName("application")]
    public string Application { get; set; }
    /// <summary>
    /// Specify if this connection can access the test environment
    /// </summary>
    public bool Test { get; set; }
  }
}