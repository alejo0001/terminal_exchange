using System;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using ErrorOr;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICourseFPApiClient
{
    [Obsolete("Temporarily out of service, but probably will be used again in future. Should not be used!", false)]
    Task<ErrorOr<Created>> DoEnrollment(WebEnrollmentDto webEnrollmentDto, CancellationToken ct);
}
