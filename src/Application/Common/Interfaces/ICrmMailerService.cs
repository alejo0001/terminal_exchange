using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using ErrorOr;
using IntranetMigrator.Domain.Common;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using NotificationAPI.Contracts.Commands;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Encapsulates business rules in terms of source data retrieval and how Customer e-mail communication should be
///     handled. It is up to the consumer to build the instance of <see cref="CreateEmail" /> to be sent.
/// </summary>
/// <remarks>
///     <para>
///         Although not prohibited, but better off use other means to send emails of non-customer communication,
///         because fulling first and foremost, this service is supposed to streamline business requirements (tracking,
///         domain data relations, etc).
///     </para>
///     <para>
///         If there's a need to have something similar for reporting or other non-customer communication, then better
///         create separate service, to keep things clear in terms of responsibilities and even more so for business
///         use case clarity standpoint.
///     </para>
/// </remarks>
public interface ICrmMailerService
{
    Task<(string subject, string body)> AddSignatureToEmailAsync(string subject, string body, CancellationToken ct);

    #region GetPricesForTLMK - ManagementApi

    Task<ErrorOr<List<(ContactLead Lead, CourseImportedTlmkDto? Price)>>> GetCoursePricesFromManagementApi(
        Process process,
        CancellationToken ct);

    Task<CourseImportedTlmkDto?> GetPricesTlmkFromManagementApiData(
        ContactLead lead,
        ProcessType processType,
        CancellationToken ct);

    #endregion

    Task<CreateEmail> BuildEmail(Process process,string receiver, string? fromEmail, string subject, string body, CancellationToken ct);
    int GetContactLanguageIdOrDefault(Process process);

    #region Get PRocess

    /// <summary>
    ///     Gets Process with all related data required to work with this service.
    /// </summary>
    /// <param name="processId"></param>
    /// <param name="ct"></param>
    /// <returns>
    ///     Materialized object graph. Non-nullable return type is to simplify consuming code, and it strongly needs
    ///     pre-validation of provided <paramref name="processId" />, to avoid blowing things up unexpectedly.
    /// </returns>
    Task<Process> GetProcessesWithRelatedData(int processId, CancellationToken ct);

    /// <summary>
    ///     <inheritdoc cref="GetProcessesWithRelatedData(int,CancellationToken)" />
    /// </summary>
    /// <param name="processId"></param>
    /// <param name="onlyContactLeadIds">
    ///     ContactLeads are limited by provided Ids; empty means no entities will be returned.
    /// </param>
    /// <param name="ct"></param>
    /// <returns>
    ///     <inheritdoc cref="GetProcessesWithRelatedData(int,CancellationToken)" />
    /// </returns>
    Task<Process> GetProcessesWithRelatedData(int processId, ICollection<int> onlyContactLeadIds, CancellationToken ct);

    Task<List<Process>> GetProcessesWithRelatedData(IEnumerable<int> processIds, CancellationToken ct);

    #endregion

    #region Get Templates

    /// <summary>
    ///     Should use internal buffer, but eventually loads <see cref="Template" /> entities with all required
    ///     related data.
    /// </summary>
    /// <param name="templateCode"></param>
    /// <param name="languageId"></param>
    /// <param name="ct"></param>
    Task<Template?> GetTemplateAsync(string templateCode, int languageId, CancellationToken ct);

    Task<Template?> GetTemplateByProcess(string label, ProcessType processType, int languageId, CancellationToken ct);

    #endregion

    #region Store Business Email Data

    Task<List<Action>> StoreBusinessEmailData(
        Dictionary<Process, CreateEmail> processEmailDictionary,
        CancellationToken ct);

    /// <summary>
    ///     <inheritdoc
    ///         cref="StoreBusinessEmailData(Process,CreateEmail,string,CancellationToken)" />
    /// </summary>
    /// <param name="process"></param>
    /// <param name="emailCommand"></param>
    /// <param name="entityCreatorTag">Will be passed to <see cref="BaseEntity.CreatedBy" /> filed</param>
    /// <param name="ct"></param>
    /// <exception cref="InvalidOperationException">
    ///     <inheritdoc cref="StoreBusinessEmailData(Process,CreateEmail,string,CancellationToken)" />
    /// </exception>
    /// <exception cref="Exception">Other exceptions, mostly form EF Core guts...</exception>
    Task<Action?> StoreBusinessEmailData(
        Process process,
        CreateEmail emailCommand,
        string entityCreatorTag,
        CancellationToken ct);

    /// <summary>
    ///     <inheritdoc
    ///         cref="StoreBusinessEmailData(IntranetMigrator.Domain.Entities.Process,NotificationAPI.Contracts.Commands.CreateEmail,string,System.Threading.CancellationToken)" />
    /// </summary>
    /// <param name="processEmailCommands"></param>
    /// <param name="entityCreatorTag">Will be passed to <see cref="BaseEntity.CreatedBy" /> filed</param>
    /// <param name="ct"></param>
    /// <exception cref="InvalidOperationException">
    ///     <inheritdoc
    ///         cref="StoreBusinessEmailData(IntranetMigrator.Domain.Entities.Process,NotificationAPI.Contracts.Commands.CreateEmail,string,System.Threading.CancellationToken)" />
    /// </exception>
    /// <exception cref="Exception">Other exceptions, mostly form EF Core guts...</exception>
    Task<List<Action>> StoreBusinessEmailData(
        Dictionary<Process, CreateEmail> processEmailCommands,
        string entityCreatorTag,
        CancellationToken ct);

    /// <summary>
    ///     <inheritdoc cref="StoreBusinessEmailData(Process,CreateEmail,string,CancellationToken)" />
    /// </summary>
    /// <param name="process"></param>
    /// <param name="emailCommand"></param>
    /// <param name="ct"></param>
    /// <exception cref="InvalidOperationException">
    ///     <inheritdoc cref="StoreBusinessEmailData(Process,CreateEmail,string,CancellationToken)" />
    /// </exception>
    /// <exception cref="Exception">Other exceptions, mostly form EF Core guts...</exception>
    Task<Action?> StoreBusinessEmailData(
        Process process,
        CreateEmail emailCommand,
        CancellationToken ct);

    /// <summary>
    ///     Creates <see cref="Email" /> and its related <see cref="Action" /> with type of
    ///     <see cref="ActionType.EmailPending" /> and adds them to DbContext.
    /// </summary>
    /// <param name="processEmailCommands"></param>
    /// <param name="entityCreatorTag">Will be passed to <see cref="BaseEntity.CreatedBy" /> filed</param>
    /// <param name="configureEntitiesDlgt">
    ///     Allows to configure created entities that where created with minimum required state. Will be executed
    ///     before saving them tu datastore.
    /// </param>
    /// <param name="ct"></param>
    /// <exception cref="InvalidOperationException">
    ///     When <paramref name="processEmailCommands" /> doesn't have
    ///     <see cref="CreateEmail.CorrelationId" /> set.
    /// </exception>
    Task<List<Action>> StoreBusinessEmailData(
        Dictionary<Process, CreateEmail> processEmailCommands,
        string entityCreatorTag,
        Action<Email, Action> configureEntitiesDlgt,
        CancellationToken ct);

    #endregion

    /// <summary>
    ///     <b>Sends</b> the command <see cref="CreateEmail" /> to messaging broker.
    /// </summary>
    /// <param name="emailCommand"></param>
    /// <param name="ct"></param>
    Task SendCreateEmailCommand(CreateEmail emailCommand, CancellationToken ct);

    Task<List<int>> SendAllEmails(Dictionary<Process, CreateEmail> processEmailCommands, CancellationToken ct);

    #region Utils

    string CreateResultMessage(int countOfEmailsSent, List<Process> processes);

    decimal SanitizeDiscountPercentage(decimal? rawDiscount);

    public Func<string?>? ConvocationDateDelegate(bool useConvocationDate = true);

    #endregion
}
