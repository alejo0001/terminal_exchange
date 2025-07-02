using System.Collections.Generic;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Contacts.Commands.ConsolidateContactsCommand;

/// <summary>
///     Used for DB projection. All properties are meant to be entity types, so they will be traced for mutation purposes.
/// </summary>
/// <remarks>
///     Idea is to interact with all the child dependencies in uniform way. Alternative would be using <c>Include()</c>,
///     but join entities still need to be queried explicitly, because it is necessary to modify their states too.
/// </remarks>
public record OriginContactDto
{
    public required Contact Contact { get; set; }

    public List<Process> Processes { get; set; } = new();

    public List<Action> Actions { get; set; } = new();

    public List<Annotation> Annotations { get; set; } = new();

    public List<IntranetMigrator.Domain.Entities.OrdersImported> OrdersImported { get; set; } = new();

    public List<ContactEmail> ContactEmails { get; set; } = new();

    public List<ContactPhone> ContactPhones { get; set; } = new();

    public List<ContactLead> ContactLeads { get; set; } = new();

    public List<ContactAddress> ContactAddresses { get; set; } = new();

    public List<ContactLeadProcess> ContactLeadProcesses { get; set; } = new();

    public List<ContactTitle> ContactTitles { get; set; } = new();

    public List<ContactSpeciality> ContactSpecialities { get; set; } = new();

    public List<ContactFaculty> ContactFaculties { get; set; } = new();

    public List<ContactLanguage> ContactLanguages { get; set; } = new();
}
