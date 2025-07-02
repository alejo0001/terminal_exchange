using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace CrmAPI.Infrastructure.Services;

/// <inheritdoc />
public class EmailSendTestingFeatureFlagService : IEmailSendTestingFeatureFlagService
{
    private readonly IOptionsMonitor<ConnectionStringsSettings> _optionsMonitor;

    public EmailSendTestingFeatureFlagService(IOptionsMonitor<ConnectionStringsSettings> optionsMonitor) =>
        _optionsMonitor = optionsMonitor;

    /// <inheritdoc />
    public bool IsAutomaticEmailSendingForcedToTestOnly => _optionsMonitor.CurrentValue.SendEmail;
}
