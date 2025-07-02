namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     REMOVE ASPA, OR REMOVE Dependency from KVS as a Config Source!
/// </summary>
/// <remarks>
///     <para>
///         It solely exists, because AZ KVS is integrated into solution, but ConnectionSettings type is not visible in
///         Application layer. So implementation mus be in Infrastructure Layer to interact with distended setting from
///         KVS.
///     </para>
///     <para>
///         Register as SingletonService!
///     </para>
/// </remarks>
public interface IEmailSendTestingFeatureFlagService
{
    public bool IsAutomaticEmailSendingForcedToTestOnly { get; }
}
