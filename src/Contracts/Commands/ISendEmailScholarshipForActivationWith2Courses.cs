using CrmAPI.Contracts.Dtos;
using MassTransit;

namespace CrmAPI.Contracts.Commands;

/// <summary>
///     Send email of "Mail de Activación de Beca ACTIVACIONES CON PROGRAMA ELEGIDO" / (A00.A).
/// </summary>
public interface ISendEmailScholarshipForActivationWith2Courses : CorrelatedBy<NewId>
{
    public SendEmailScholarshipForActivationWith2CoursesDto Dto { get; }
}
