using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces;

public interface IPotentialsService
{
    Task CreateOrUpdateContactInPotentials(int contactId, CancellationToken cancellationToken, [Optional] int? couponOrigin);

    Task UpdateStatusContactInPotentials(int originalContactId, int status, CancellationToken ct);
}
