using System.Text.Json.Serialization;

namespace GroupClaes.OpenEdge.Connector.Shared.Models
{
  public class Parameter
  {
    /// <summary>
    /// Position index of the parameter
    /// </summary>
    /// <remark>The index is always 1-based and thus does not start at 0</remark>
    [JsonPropertyName("pos")]
    public int Position { get; set; }
    /// <summary>
    /// The value to be entered at the position when executing the procedure
    /// </summary>
    public object Value { get; set; }
    /// <summary>
    /// Should redact the contents from any logging
    /// </summary>
    public bool Redact { get; set; }
    /// <summary>
    /// Specify whether or not the input is an output
    /// </summary>
    /// <value></value>
    [JsonPropertyName("out")]
    public bool Output { get; set; }

    public static Parameter CreateOutput(int position)
      => new Parameter
      {
        Position = position,
        Output = true
      };
  }
}