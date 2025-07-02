using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using ErrorOr;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Sending new enrollment to Web.
/// </summary>
public interface ICourseWebEnrollmentService
{
    Task<ErrorOr<Created>> SendNewWebEnrollment(WebEnrollmentDto serviceDto, CancellationToken ct);
}
