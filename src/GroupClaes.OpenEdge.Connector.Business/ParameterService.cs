﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using GroupClaes.OpenEdge.Connector.Business.Extensions;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Progress.Open4GL.DynamicAPI;

namespace GroupClaes.OpenEdge.Connector.Business
{
  internal class ParameterService : IParameterService
  {
    private readonly IChecksumService checksumService;
    private readonly IJsonSerializer jsonSerializer;

    public ParameterService(IChecksumService checksumService, IJsonSerializer jsonSerializer)
    {
      this.checksumService = checksumService;
      this.jsonSerializer = jsonSerializer;
    }

    public Parameter[] GetFilteredParameters(IEnumerable<Parameter> requestParameters,
      out bool hasRedacted, out string parameterHash)
    {
      hasRedacted = false;
      if (requestParameters.Count() == 0)
      {
        parameterHash = null;
        return Array.Empty<Parameter>();
      }

      Parameter[] parameters = requestParameters
        .OrderBy(x => x.Position)
        .ToArray();
      StringBuilder hashBuilder = new StringBuilder(requestParameters.Count() * 64);
      Parameter parameter;
      for (int i = 0; i < parameters.Length; i++)
      {
        parameter = parameters[i];
        if (!parameter.Output)
        {
          if (parameter.Value.ValueKind != JsonValueKind.Null)
          {
            hashBuilder.Append(parameter.Position);
            hashBuilder.Append(parameter.Value);
          }

          if (parameter.Redact)
          {
            parameters[i] = RedactParameter(parameter);
            hasRedacted = true;
          }
        }
      }

      parameterHash = hashBuilder.Length > 0 ? checksumService.Generate(hashBuilder) : null;
      return parameters;
    }

    public object ExtractAndParseValue(Parameter parameter, ParameterSet parameterSet)
    {
      // We have to get it out of the parameter set because Progress... Grrrrr....
      object value = parameterSet.getOutputParameter(parameter.Position);
      if (value is Progress.Open4GL.Memptr pointer)
      {
        if (parameter.Type == ParameterType.JSON)
        {
          if (pointer.Bytes != null && pointer.Bytes.Any())
          {
            JsonDocument result = JsonDocument.Parse(pointer.Bytes);
            // Check if the element is an array, array isn't forced and length is 1
            if (result.RootElement.ValueKind == JsonValueKind.Array
               && !parameter.ForceArray && result.RootElement.GetArrayLength() == 1)
            {
              return result.RootElement.EnumerateArray()
                  .First();
            }

            return result;
          }
          else
          {
            return null;
          }
        }

        return pointer.Bytes;
      }
      else if (value is string toParse && parameter.Type == ParameterType.LongChar)
      {
        return JsonDocument.Parse(toParse);
      }

      return value;
    }

    public ParameterSet GenerateParameterSet(IEnumerable<Parameter> parameters)
    {
      ParameterSet parameterSet = new ParameterSet(parameters.Count());
      foreach (Parameter parameter in parameters)
      {
        if (parameter.Value.ValueKind != JsonValueKind.Null)
        {
          int inputOutputType = parameter.Output ? ParameterSet.OUTPUT : ParameterSet.INPUT;
          if (!parameter.Output && parameter.Type == ParameterType.JSON)
          {
            byte[] valueArray = jsonSerializer.SerializeToBytes(parameter.Value);
            parameterSet.setParameter(parameter.Position, new Progress.Open4GL.Memptr(valueArray), inputOutputType,
              parameter.GetParameterSetType(), false, 0, null);
          }
          else if (parameter.Value.ValueKind == JsonValueKind.Array)
          {
            object[] values = (object[])parameter.ExtractValue();
            parameterSet.setParameter(parameter.Position, values, inputOutputType,
                parameter.GetParameterSetType(), (values.Length > 0), values.Length, null);
          }
          else
          {
            parameterSet.setParameter(parameter.Position, parameter.ExtractValue(),
                inputOutputType, parameter.GetParameterSetType(), false, 0, null);
          }
        }
        else if (!parameter.Output)
        {
          // Set input to NULL
          parameterSet.setParameter(parameter.Position, null,
              ParameterSet.INPUT, parameter.GetParameterSetType(), false, 0, null);
        }
        else
        {
          parameterSet.setParameter(parameter.Position, null, ParameterSet.OUTPUT,
              parameter.GetParameterSetType(), false, 0, null);
        }
      }

      return parameterSet;
    }

    /// <summary>
    /// Extract all procedure responses from the ouput parameters and label them if applicable.
    /// </summary>
    /// <param name="requestParameters">List of requested parameters.</param>
    /// <param name="parameters">Parameterset containing the procedure response values</param>
    /// <returns>A dictionary of the output values, keyed based on the label or fallback of the position</returns>
    public Dictionary<string, object> GetOutputParameters(IEnumerable<Parameter> requestParameters, ParameterSet parameters)
    {
      requestParameters = requestParameters.Where(x => x.Output);
      if (requestParameters.Count() == 1)
      {
        Parameter parameter = requestParameters.First();
        if (!parameter.HasLabel)
        {
          return new Dictionary<string, object>
          {
            { string.Empty, ExtractAndParseValue(parameter, parameters) }
          };
        }
      }

      return requestParameters.ToDictionary(x => x.ResponseLabel, x => ExtractAndParseValue(x, parameters));
    }

    public object GetParsedOutputs(Dictionary<string, object> outputsDictionary)
    {
      if (outputsDictionary.Count == 1)
      {
        var result = outputsDictionary.First();
        if (result.Key == string.Empty)
        {
          return result.Value;
        }
      }

      return outputsDictionary;
    }

    public Parameter RedactParameter(Parameter parameter)
    {
      return new Parameter
      {
        Position = parameter.Position,
        Label = parameter.Label,
        Output = parameter.Output,
        Value = jsonSerializer.ParseJsonElement("***"),
        Redact = true
      };
    }
  }
}
