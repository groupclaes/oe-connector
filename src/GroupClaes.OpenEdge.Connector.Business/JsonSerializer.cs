using System.Text.Json;

namespace GroupClaes.OpenEdge.Connector.Business
{
  internal class JsonSerializer : IJsonSerializer
  {
    private readonly JsonSerializerOptions serializerOptions;

    public JsonSerializer()
    {
      serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public T DeserializeBytes<T>(byte[] bytes)
    {
      return System.Text.Json.JsonSerializer.Deserialize<T>(bytes, serializerOptions);
    }

    public string Serialize<T>(T value)
    {
      return System.Text.Json.JsonSerializer.Serialize(value, serializerOptions);
    }

    public byte[] SerializeToBytes<T>(T value)
    {
      return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, serializerOptions);
    }

    public JsonElement ParseJsonElement<T>(T value)
    {
      return System.Text.Json.JsonSerializer.SerializeToElement(value);
    }
  }
}
