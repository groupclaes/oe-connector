using GroupClaes.OpenEdge.Connector.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GroupClaes.OpenEdge.Connector.Business.Extensions
{
  public static class ParameterExtensions
  {
    /// <summary>
    /// Retrieve the parameter set type for OpenEdge from the input parameter
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static int GetParameterSetType(this Parameter parameter)
    {
      if (parameter.Type == ParameterType.Undefined)
      {
        if (parameter.Output)
        {
          // Json is the default output parameter
          parameter.Type = ParameterType.JSON;
          return (int)ParameterType.MemPointer;
        }
        else
        {
          // Default input parameter
          parameter.Type = ParameterType.String;
        }
      }
      else if (parameter.Type == ParameterType.JSON)
      {
        // JSON should become a mempointer
        return (int)ParameterType.MemPointer;
      }

      return (int)parameter.Type;
    }

    /// <summary>
    /// Extract a .NET object from the JsonElement in the parameter
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static object ExtractValue(this Parameter parameter)
      => ExtractValueFromJsonElement(parameter.Value);

    private static object ExtractValueFromJsonElement(JsonElement value)
    {
      switch (value.ValueKind)
      {
        case JsonValueKind.Array:
          object[] values = value.EnumerateArray()
            .Select(x => ExtractValueFromJsonElement(x))
            .ToArray();
          return values;
        case JsonValueKind.True:
          return true;
        case JsonValueKind.False:
          return false;
        case JsonValueKind.Number:
          return value.GetInt32();
        case JsonValueKind.Object:
          Dictionary<string, object> objectProperties = value.EnumerateObject()
              .ToDictionary(x => x.Name, x => ExtractValueFromJsonElement(x.Value));
          return objectProperties;
        case JsonValueKind.Undefined:
        case JsonValueKind.Null:
          return null;
        default:
          return value.GetString();
      }
    }
  }
}
