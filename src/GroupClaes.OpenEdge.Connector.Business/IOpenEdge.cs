using System.Threading;
using System.Threading.Tasks;
using GroupClaes.OpenEdge.Connector.Shared;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public interface IOpenEdge
  {
    Task<byte[]> ExecuteProcedureAsync(ProcedureRequest request, CancellationToken cancellationToken = default);
  }
}
