using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GroupClaes.OpenEdge.Connector.Shared.Models
{
  public class Parameter
  {
    /// <summary>
    /// Position index of the parameter
    /// </summary>
    /// <remark>The index is always 1-based and thus does not start at 0</remark>
    [JsonPropertyName("pos")]
    [Range(1, 999)]
    public int Position { get; set; }
    /// <summary>
    /// Label name to rename output position key to
    /// </summary>
    public string Label { get; set; }
    /// <summary>
    /// The value to be entered at the position when executing the procedure
    /// </summary>
    public object Value { get; set; }
    /// <summary>
    /// Specify the type of value(s) provided
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ParameterType Type { get; set; }
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
    [JsonPropertyName("ar")]
    public bool ForceArray { get; set; }

    [JsonIgnore]
    public string ResponseLabel
    {
      get => HasLabel ? Label : Position.ToString();
    }

    [JsonIgnore]
    public bool HasLabel { get => Label != null && Regexes.Label.IsMatch(Label); }

    public static Parameter CreateOutput(int position)
      => new Parameter
      {
        Position = position,
        Output = true
      };
    
    public Parameter RedactCopy()
      => new Parameter
      {
        Position = this.Position,
        Label = this.Label,
        Output = this.Output,
        Value = "***",
        Redact = true
      };
  }
}