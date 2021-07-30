using System.Threading;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public interface IOpenEdge
  {
    Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default);
    Task<ProcedureResponse> GetProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default);
    bool GetFilteredParameters(ProcedureRequest request, out Parameter[] displayeableFilters, out string parameterHash);
  }
}
