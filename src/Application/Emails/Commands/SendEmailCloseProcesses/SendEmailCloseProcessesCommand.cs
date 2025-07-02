using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Emails.Commands.SendEmailCloseProcesses;

public record SendEmailCloseProcessesCommand : Contracts.Commands.SendEmailCloseProcesses, IRequest<string>
{
    public required string ApiKey { get; init; }
}

[UsedImplicitly]
public class SendEmailCloseProcessesCommandHandler : IRequestHandler<SendEmailCloseProcessesCommand, string>
{
    private readonly ICloseProcessService _closeProcessService;
    private readonly ILogger<SendEmailCloseProcessesCommandHandler> _logger;

    public SendEmailCloseProcessesCommandHandler(
        ICloseProcessService closeProcessService,
        IConfiguration configuration,
        ILogger<SendEmailCloseProcessesCommandHandler> logger)
    {
        _closeProcessService = closeProcessService;
        _logger = logger;
    }

    public async Task<string> Handle(SendEmailCloseProcessesCommand request, CancellationToken ct) =>
        await _closeProcessService.EmailCloseProcessStepByStep(request.ProcessIds, ct);
}
