using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using System;

namespace GroupClaes.OpenEdge.Connector.Business
{
  internal class ProcedureParser : IProcedureParser
  {
    private readonly IJsonSerializer jsonSerializer;

    public ProcedureParser(IJsonSerializer jsonSerializer)
    {
      this.jsonSerializer = jsonSerializer;
    }

    public ProcedureResult GetProcedureResult(string returnValue)
    {
      string[] returnCode = returnValue.Split(new string[] { "::" }, 3, StringSplitOptions.None);
      if (returnCode.Length > 1)
      {
        ProcedureResult result = new ProcedureResult();
        if (Regexes.HttpStatusCode.IsMatch(returnCode[0]))
        {
          result.StatusCode = int.Parse(returnCode[0]);
          result.Title = returnCode[1];

          if (returnCode.Length == 3)
          {
            result.Description = returnCode[2];
          }

          return result;
        }
      }

      return null;
    }

    public ProcedureErrorResponse GetErrorResponse(ProcedureResponse response, ProcedureResult result)
    {
      ProcedureErrorResponse errorResponse = new ProcedureErrorResponse
      {
        Status = result.StatusCode,
        Description = result.Description,
        Title = result.Title,

        Procedure = response.Procedure,
        LastModified = response.LastModified,
        OriginTime = response.OriginTime,
        Result = response.Result
      };

      return errorResponse;
    }


    public ProcedureErrorResponse GetErrorResponse(int status, string procedure, long originTime, ProcedureResult result)
    {
      ProcedureErrorResponse errorResponse = new ProcedureErrorResponse
      {
        Status = result.StatusCode,
        Description = result.Description,
        Title = result.Title,

        Procedure = procedure,
        LastModified = DateTime.UtcNow,
        OriginTime = originTime,
        Result = result
      };

      return errorResponse;
    }

    public byte[] GetProcedureResponseBytes(ProcedureResponse response)
    {
      if (response is ProcedureErrorResponse errorResponse) {
        return jsonSerializer.SerializeToBytes<ProcedureErrorResponse>(errorResponse);
      }

      return jsonSerializer.SerializeToBytes(response);
    }
  }
}
