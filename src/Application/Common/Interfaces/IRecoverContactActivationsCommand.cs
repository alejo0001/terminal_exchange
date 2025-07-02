using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces
{
    public interface IRecoverContactActivationsCommand
    {
        Task UpdateProcess(int processId, CancellationToken ct);
        Task UpdateAction(int processId, CancellationToken ct);
    }
}
