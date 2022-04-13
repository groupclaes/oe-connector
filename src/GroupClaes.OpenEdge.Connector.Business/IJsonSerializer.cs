using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public interface IJsonSerializer
  {
    /// <summary>
    /// Deserialize a JSON binary array to the desired model
    /// </summary>
    /// <typeparam name="T">Modeltype to parse to</typeparam>
    /// <param name="bytes">JSON Byte array containing the UTF-8 json data</param>
    /// <returns>The parsed object</returns>
    T DeserializeBytes<T>(byte[] bytes);
    /// <summary>
    /// Serialize an object of type T to a json string
    /// </summary>
    /// <typeparam name="T">Modeltype to parse from</typeparam>
    /// <param name="value">Model to be parsed</param>
    /// <returns>A json UTF-8 string serialized from the model</returns>
    string Serialize<T>(T value);
    /// <summary>
    /// Serialize an object of type T to a UTF-8 json byte array
    /// </summary>
    /// <typeparam name="T">Modeltype to parse from</typeparam>
    /// <param name="value">Model to be parsed</param>
    /// <returns>A json UTF-8 byte array serialized from the model</returns>
    byte[] SerializeToBytes<T>(T value);
    /// <summary>
    /// Serialize an object of type T to a UTF-8 json byte array
    /// </summary>
    /// <typeparam name="T">Modeltype to parse from</typeparam>
    /// <param name="value">Model to be parsed</param>
    /// <param name="encoding">Encoding to encode the UTF8 JSON array to</param>
    /// <returns>A json byte array serialized from the model encoded into the desired encoding</returns>
    byte[] SerializeToBytes<T>(T value, Encoding encoding);
    /// <summary>
    /// Convert a value to a json element
    /// </summary>
    /// <typeparam name="T">Valuetype  to be converted</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    JsonElement ParseJsonElement<T>(T value);
  }
}
