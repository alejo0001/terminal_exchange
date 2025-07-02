using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Actions.Queries.GetActionsSendEmailDayZero;

[UsedImplicitly]
public class CheckHasSentEmailsQueryValidator : AbstractValidator<CheckHasSentEmailsQuery>
{
    private readonly IApplicationDbContext _context;

    public CheckHasSentEmailsQueryValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ProcessId)
            .NotEmpty()
            .MustAsync(CheckExistProcess)
            .WithMessage("There is no process for the given Id.");
    }

    private async Task<bool> CheckExistProcess(int processId, CancellationToken ct) =>
        await _context.Processes
            .Where(x => x.Id == processId)
            .Where(x => !x.IsDeleted)
            .AnyAsync(ct);
}
