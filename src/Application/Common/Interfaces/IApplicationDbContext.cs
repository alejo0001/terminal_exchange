using System;
using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Common;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Action = IntranetMigrator.Domain.Entities.Action;
using Annotation = IntranetMigrator.Domain.Entities.Annotation;

namespace CrmAPI.Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    DbSet<Contact> Contact { get; set; }
    DbSet<Employee> Employees { get; set; }
    DbSet<Absence> Absences { get; set; }
    DbSet<Process> Processes { get; set; }
    DbSet<Action> Actions { get; set; }
    DbSet<Appointment> Appointments { get; set; }
    DbSet<Annotation> Annotations { get; set; }
    DbSet<TraceRecord> TraceRecords { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<IntranetMigrator.Domain.Entities.OrdersImported> OrdersImported { get; set; }
    DbSet<InvoicePaymentType> InvoicePaymentTypes { get; set; }
    DbSet<InvoicePaymentOption> InvoicePaymentOptions { get; set; }
    DbSet<ContactInvoicePaymentOption> ContactInvoicePaymentOption { get; set; }
    DbSet<Email> Emails { get; set; }

    [Obsolete("Should be in graceful removal period, because data comes from another entity.")]
    DbSet<EmailTemplate> EmailTemplates { get; set; }

    DbSet<Attachment> Attachments { get; set; }
    DbSet<EmailAttachment> EmailAttachments { get; set; }
    DbSet<Country> Country { get; set; }
    DbSet<AddressType> AddressType { get; set; }
    DbSet<EmailType> EmailType { get; set; }
    DbSet<PhoneType> PhoneType { get; set; }
    DbSet<ContactTitle> ContactTitles { get; set; }
    DbSet<ContactLead> ContactLeads { get; set; }
    DbSet<ContactPhone> ContactPhone { get; set; }
    DbSet<ContactAddress> ContactAddress { get; set; }
    DbSet<ContactEmail> ContactEmail { get; set; }
    DbSet<ContactStatus> ContactStatus { get; set; }
    DbSet<ContactGender> ContactGender { get; set; }
    DbSet<ContactType> ContactType { get; set; }
    DbSet<ContactLeadProcess> ContactLeadProcesses { get; set; }
    DbSet<CouponOrigin> CouponOrigins { get; set; }
    DbSet<ContactCourse> ContactCourse { get; set; }
    DbSet<Speciality> Specialities { get; set; }
    DbSet<Faculty> Faculties { get; set; }
    DbSet<InvoicePaymentType> InvoicePaymentType { get; set; }
    DbSet<InvoicePaymentMethod> InvoicePaymentMethod { get; set; }
    DbSet<Currency> Currency { get; set; }
    DbSet<PaymentState> PaymentState { get; set; }
    DbSet<Reassignment> Reassignments { get; set; }
    DbSet<RemittanceState> RemittanceStates { get; set; }
    DbSet<CountryFacultySpeciality> CountryFacultySpecialities { get; set; }
    DbSet<Course> Courses { get; set; }
    DbSet<Guarantor> Guarantors { get; set; }
    DbSet<CourseType> CourseTypes { get; set; }
    DbSet<CourseFaculty> CourseFaculties { get; set; }
    DbSet<CourseSpeciality> CourseSpecialities { get; set; }
    DbSet<CourseGuarantor> CourseGuarantors { get; set; }
    DbSet<OrganizationNode> OrganizationNodes { get; set; }
    DbSet<CompanyCountry> CompanyCountries { get; set; }
    DbSet<TitleType> TitleTypes { get; set; }
    DbSet<Language> Languages { get; set; }
    DbSet<ContactLanguage> ContactLanguages { get; set; }
    DbSet<CourseCountry> CourseCountries { get; set; }
    DbSet<ContactFaculty> ContactFaculty { get; set; }
    DbSet<ContactSpeciality> ContactSpeciality { get; set; }
    DbSet<UserRole> UserRoles { get; set; }
    DbSet<FacultySpeciality> FacultySpecialities { get; set; }
    DbSet<CountryFaculty> CountryFaculties { get; set; }
    DbSet<CourseData> CourseData { get; set; }
    DbSet<CourseDataGuarantor> CourseDataGuarantors { get; set; }
    DbSet<Whatsapp> Whatsapps { get; set; }

    [Obsolete("Should be in graceful removal period, because data comes from another entity.")]
    DbSet<WhatsappTemplate> WhatsappTemplates { get; set; }

    DbSet<TemplatesFlow> TemplatesFlows { get; set; }
    DbSet<TemplateProposal> TemplateProposals { get; set; }
    DbSet<OrganizationNodeTag> OrganizationNodeTags { get; set; }
    DbSet<Tag> Tags { get; set; }
    DbSet<WebEnrollment> WebEnrollments { get; set; }
    DbSet<TemplateProposalTemplate> TemplateProposalTemplates { get; set; }
    DbSet<ProcessSetting> ProcessSettings { get; set; }
    DbSet<Template> Templates { get; set; }
    DbSet<CourseTypeBase> CourseTypeBases { get; set; }
    DbSet<DiscardReason> DiscardReasons { get; set; }
    DbSet<DiscardReasonProcess> DiscardReasonProcesses { get; set; }
    DbSet<TopSellingCourse> TopSellingCourses { get; set; }
    DbSet<AzureGroup> AzureGroups { get; set; }
    DbSet<OrganizationNodeAzureGroup> OrganizationNodeAzureGroups { get; set; }
    DbSet<EmployeeAzureGroup> EmployeeAzureGroups { get; set; }
    DbSet<CrmCatalogTable> CrmCatalogTables { get; set; }
    DbSet<CrmCatalogTableField> CrmCatalogTableFields { get; set; }
    DbSet<CrmCatalogTableFieldValue> CrmCatalogTableFieldValues { get; set; }
    
    DatabaseFacade Database { get; }
    
    EntityEntry<TEntity> Entry<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <inheritdoc cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)" />
    /// <summary>
    ///     Allows to "inject" creator information as a tag to <see cref="BaseEntity.CreatedBy" /> to identify caller.<br />
    ///     <inheritdoc cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)" />
    /// </summary>
    /// <remarks>
    ///     It is up to implementation, how this information is used, combined, etc.
    ///     <inheritdoc cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)" /><br />
    /// </remarks>
    /// <param name="creatorTag"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(string creatorTag, CancellationToken cancellationToken);

    /// <summary>
    ///     Allows to clear context/repository current tacked state. It should be worth to be used on flows, where it's
    ///     practical to use same underlying context instance around for prolonged operations and accumulated, but
    ///     <em>unuseful state</em> is held in context.
    /// </summary>
    void ClearUnitOfWork();
}
