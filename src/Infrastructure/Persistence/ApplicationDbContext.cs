using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using IntranetMigrator.Domain.Common;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using IntranetMigrator.Persistence;
using Microsoft.EntityFrameworkCore;
using Action = IntranetMigrator.Domain.Entities.Action;
using IHasDomainEvent = CrmAPI.Domain.Common.IHasDomainEvent;

namespace CrmAPI.Infrastructure.Persistence;

/// <inheritdoc cref="DbContext" />
/// <inheritdoc cref="IApplicationDbContext" />
public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork<IApplicationDbContext>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IDomainEventService _domainEventService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDomainEventService domainEventService,
        IDateTime dateTime) : base(options)
    {
        _currentUserService = currentUserService;
        _domainEventService = domainEventService;
        _dateTime = dateTime;
    }

    public DbSet<Contact> Contact { get; set; }
    public DbSet<Process> Processes { get; set; }
    public DbSet<Action> Actions { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Annotation> Annotations { get; set; }
    public DbSet<TraceRecord> TraceRecords { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Absence> Absences { get; set; }
    public DbSet<OrdersImported> OrdersImported { get; set; }
    public DbSet<Enterprise> Enterprises { get; set; }
    public DbSet<EnterpriseBranchOffice> EnterpriseBranchOffices { get; set; }
    public DbSet<InvoicePaymentType> InvoicePaymentTypes { get; set; }
    public DbSet<InvoicePaymentOption> InvoicePaymentOptions { get; set; }
    public DbSet<ContactInvoicePaymentOption> ContactInvoicePaymentOption { get; set; }
    public DbSet<Email> Emails { get; set; }

    [Obsolete("Should be in graceful removal period, because data comes from another entity.")]
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<EmailAttachment> EmailAttachments { get; set; }
    public DbSet<Country> Country { get; set; }
    public DbSet<AddressType> AddressType { get; set; }
    public DbSet<EmailType> EmailType { get; set; }
    public DbSet<PhoneType> PhoneType { get; set; }
    public DbSet<ContactTitle> ContactTitles { get; set; }
    public DbSet<ContactLead> ContactLeads { get; set; }
    public DbSet<ContactPhone> ContactPhone { get; set; }
    public DbSet<ContactAddress> ContactAddress { get; set; }
    public DbSet<ContactEmail> ContactEmail { get; set; }
    public DbSet<ContactStatus> ContactStatus { get; set; }
    public DbSet<ContactGender> ContactGender { get; set; }
    public DbSet<ContactType> ContactType { get; set; }
    public DbSet<ContactLeadProcess> ContactLeadProcesses { get; set; }
    public DbSet<ContactCourse> ContactCourse { get; set; }
    public DbSet<Speciality> Specialities { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<InvoicePaymentType> InvoicePaymentType { get; set; }
    public DbSet<InvoicePaymentMethod> InvoicePaymentMethod { get; set; }
    public DbSet<Currency> Currency { get; set; }
    public DbSet<PaymentState> PaymentState { get; set; }
    public DbSet<Reassignment> Reassignments { get; set; }
    public DbSet<RemittanceState> RemittanceStates { get; set; }
    public DbSet<CountryFacultySpeciality> CountryFacultySpecialities { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Guarantor> Guarantors { get; set; }
    public DbSet<CourseType> CourseTypes { get; set; }
    public DbSet<CourseFaculty> CourseFaculties { get; set; }
    public DbSet<CourseSpeciality> CourseSpecialities { get; set; }
    public DbSet<CourseGuarantor> CourseGuarantors { get; set; }
    public DbSet<OrganizationNode> OrganizationNodes { get; set; }
    public DbSet<CompanyCountry> CompanyCountries { get; set; }
    public DbSet<TitleType> TitleTypes { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<CourseCountry> CourseCountries { get; set; }
    public DbSet<ContactLanguage> ContactLanguages { get; set; }
    public DbSet<ContactFaculty> ContactFaculty { get; set; }
    public DbSet<CouponOrigin> CouponOrigins { get; set; }
    public DbSet<ContactSpeciality> ContactSpeciality { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<FacultySpeciality> FacultySpecialities { get; set; }
    public DbSet<CountryFaculty> CountryFaculties { get; set; }
    public DbSet<CourseData> CourseData { get; set; }
    public DbSet<CourseDataGuarantor> CourseDataGuarantors { get; set; }
    public DbSet<Whatsapp> Whatsapps { get; set; }

    [Obsolete("Should be in graceful removal period, because data comes from another entity.")]
    public DbSet<WhatsappTemplate> WhatsappTemplates { get; set; }

    public DbSet<TemplatesFlow> TemplatesFlows { get; set; }
    public DbSet<TemplateProposal> TemplateProposals { get; set; }
    public DbSet<WebEnrollment> WebEnrollments { get; set; }
    public DbSet<TemplateProposalTemplate> TemplateProposalTemplates { get; set; }
    public DbSet<ProcessSetting> ProcessSettings { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<TopSellingCourse> TopSellingCourses { get; set; }
    public DbSet<AzureGroup> AzureGroups { get; set; }
    public DbSet<OrganizationNodeAzureGroup> OrganizationNodeAzureGroups { get; set; }
    public DbSet<EmployeeAzureGroup> EmployeeAzureGroups { get; set; }
    public DbSet<CrmCatalogTable> CrmCatalogTables { get; set; }
    public DbSet<CrmCatalogTableField> CrmCatalogTableFields { get; set; }
    public DbSet<CrmCatalogTableFieldValue> CrmCatalogTableFieldValues { get; set; }
    public DbSet<OrganizationNodeTag> OrganizationNodeTags { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<CourseTypeBase> CourseTypeBases { get; set; }
    public DbSet<DiscardReason> DiscardReasons { get; set; }
    public DbSet<DiscardReasonProcess> DiscardReasonProcesses { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(string.Empty, cancellationToken);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(string creatorTag, CancellationToken cancellationToken)
    {
        var creator = PersistenceAuditingHelper.InjectCreatorPartialAsTag(_currentUserService.Email, creatorTag);

        List<TraceRecord> traceRecords = new List<TraceRecord>();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry in ChangeTracker
                     .Entries<BaseEntity>())
        {
            bool creation = false;
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = creator;
                    entry.Entity.Created = _dateTime.Now;
                    creation = true;

                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = creator;
                    entry.Entity.LastModified = _dateTime.Now;
                    break;
            }
            //traceRecords.AddRange(await GetTraceRecords(entry.Entity, creation));
        }

        var result = await base.SaveChangesAsync(cancellationToken);
        if (traceRecords.Any())
        {
            TraceRecords.AddRange(traceRecords);
            await base.SaveChangesAsync(cancellationToken);
        }

        await DispatchEvents();

        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(BaseIntranetDbContext).Assembly);

        base.OnModelCreating(builder);
    }

    private async Task DispatchEvents()
    {
        while (true)
        {
            var domainEventEntity = ChangeTracker
                .Entries<IHasDomainEvent>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x)
                .FirstOrDefault(domainEvent => !domainEvent.IsPublished);
            if (domainEventEntity == null) break;

            domainEventEntity.IsPublished = true;
            await _domainEventService.Publish(domainEventEntity);
        }
    }

    private async Task<List<TraceRecord>> GetTraceRecords(BaseEntity entity, bool creation = false)
    {
        var modEntity = Entry(entity).CurrentValues.ToObject();
        var originalEntity = Entry(entity).OriginalValues.ToObject();
        User user = await Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email);

        List<TraceRecord> traceRecords = new List<TraceRecord>();

        Type type = modEntity.GetType();
        var idValue = type.GetProperty("Id")?.GetValue(modEntity);
        int entityId = idValue != null ? (int) idValue : 0;
        if (creation)
        {
            traceRecords.Add(new TraceRecord()
            {
                EntityId = entityId,
                EntityType = type.Name,
                UserId = user.Id,
                Date = _dateTime.Now,
                Property = "",
                PreviousValue = "",
                NewValue = "",
                Type = TraceRecordType.Created
            });
        }
        else
        {
            PropertyInfo[] properties = type.GetProperties();


            foreach (PropertyInfo property in properties)
            {
                if (
                    !property.Name.Contains("Modified")
                    && property.Name != "Created"
                    && property.Name != "IsDeleted"
                )
                {
                    var oldValue = property.GetValue(originalEntity, null)?.ToString();
                    var newValue = property.GetValue(modEntity, null)?.ToString();
                    if (newValue != oldValue && entityId > 0)
                    {
                        traceRecords.Add(new TraceRecord()
                        {
                            EntityId = entityId,
                            EntityType = type.Name,
                            UserId = user.Id,
                            Date = _dateTime.Now,
                            Property = property.Name,
                            PreviousValue = oldValue,
                            NewValue = newValue,
                            Type = TraceRecordType.Modified
                        });
                    }
                }
                // Deleted
                if (property.Name == "IsDeleted")
                {
                    var oldValue = property.GetValue(originalEntity, null)?.ToString();
                    var newValue = property.GetValue(modEntity, null)?.ToString();
                    if (newValue != oldValue && entityId > 0)
                    {
                        TraceRecordType typeAux = TraceRecordType.Deleted;
                        if (oldValue == "true" && newValue == "false")
                        {
                            typeAux = TraceRecordType.Recovered;
                        }
                        traceRecords.Add(new TraceRecord()
                        {
                            EntityId = entityId,
                            EntityType = type.Name,
                            UserId = user.Id,
                            Date = _dateTime.Now,
                            Property = property.Name,
                            PreviousValue = oldValue,
                            NewValue = newValue,
                            Type = typeAux
                        });
                    }
                }
            }
        }
        return traceRecords;
    }

    void IApplicationDbContext.ClearUnitOfWork() => base.ChangeTracker.Clear();
}
