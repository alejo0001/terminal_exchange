using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Queries.GetSuggestedNextInteractionDate;

/// <param name="DateLocalEmployee">
///     Date Local Time with unspecified type Zone (without HourZone). Expected:
///     2024-09-03T09:34:56.789
/// </param>
[Authorize]
public record GetSuggestedNextInteractionDateQuery(int ProcessId, string DateLocalEmployee) : IRequest<DateTime?>;

[UsedImplicitly]
public class GetSuggestedNextInteractionDateQueryHandler
    : IRequestHandler<GetSuggestedNextInteractionDateQuery, DateTime?>
{
    private readonly IProcessesService _processesService;
    private readonly IApplicationDbContext _context;

    public GetSuggestedNextInteractionDateQueryHandler(
        IProcessesService processesService,
        IApplicationDbContext context)
    {
        _processesService = processesService;
        _context = context;
    }

    public async Task<DateTime?> Handle(GetSuggestedNextInteractionDateQuery request, CancellationToken ct)
    {
        var process = await _context.Processes
            .Where(p => p.Id == request.ProcessId && !p.IsDeleted)
            .FirstOrDefaultAsync(ct);

        // (ES) Si no existe, no tiene sentido seguir ejecutando y debería devolver null
        // ------------------------------------------------------------------------------------
        // (EN) If it does not exist, there is no point in continuing execution and a null value would be returned
        if (process is null)
        {
            return null;
        }

        if (!DateTime.TryParse(
                request.DateLocalEmployee,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var localEmployeeDateTime))
        {
            return null;
        }

        return await _processesService.GetNextInteractionDateWhenThereIsNoResponse(process, localEmployeeDateTime, ct);
    }
}
