using System.Threading;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public interface IOpenEdge
  {
    Task<byte[]> ExecuteProcedureWithTimeoutAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default);
    Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default);
    Task<ProcedureResponse> GetProcedureAsync(ProcedureRequest request, string parameterHash = null, bool isTest = false, CancellationToken cancellationToken = default);
  }
}
