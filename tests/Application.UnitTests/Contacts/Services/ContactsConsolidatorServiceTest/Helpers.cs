using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Domain.Leads.Entities;
using ErrorOr;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;
using static NSubstitute.Arg;
using static NSubstitute.Substitute;
using Action = IntranetMigrator.Domain.Entities.Action;
using Annotation = IntranetMigrator.Domain.Entities.Annotation;

namespace Application.UnitTests.Contacts.Services.ContactsConsolidatorServiceTest;

public static class Helpers
{
    public static (IApplicationDbContext, StubEntityClonerService<IApplicationDbContext>, ILeadsDbContext )
        BuildContextMocks()
    {
        var intranetDbContextMock = For<IApplicationDbContext, IUnitOfWork<IApplicationDbContext>, DbContext>();

        var stubService = new StubEntityClonerService<IApplicationDbContext>(
            (intranetDbContextMock as IUnitOfWork<IApplicationDbContext>)!);

        var leadsDbContextMock = For<ILeadsDbContext, DbContext>();

        var intranetFacadeMock = For<DatabaseFacade>(intranetDbContextMock as DbContext);
        var leadsFacadeMock = For<DatabaseFacade>(leadsDbContextMock as DbContext);

        intranetDbContextMock.Database
            .Returns(intranetFacadeMock);

        leadsDbContextMock.Database
            .Returns(leadsFacadeMock);

        return (intranetDbContextMock, stubService, leadsDbContextMock);
    }

    /// <summary>
    ///     Only creates and setups essentials in terms of needed test data graph and dependencies.
    /// </summary>
    /// <returns></returns>
    public static (
        IApplicationDbContext,
        StubEntityClonerService<IApplicationDbContext>,
        ILeadsDbContext,
        IDbContextTransaction intranetTransactionMock,
        IDbContextTransaction leadsTransactionMock)
        BuildContextAndTranMocks()
    {
        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks();

        var intranetFacadeMock = For<DatabaseFacade>(intranetDbContextMock as DbContext);
        var leadsFacadeMock = For<DatabaseFacade>(leadsDbContextMock as DbContext);

        var intranetTransactionMock = For<IDbContextTransaction>();
        var leadsTransactionMock = For<IDbContextTransaction>();

        intranetDbContextMock.Database
            .Returns(intranetFacadeMock);

        leadsDbContextMock.Database
            .Returns(leadsFacadeMock);

        intranetFacadeMock.BeginTransactionAsync(Any<CancellationToken>())
            .Returns(intranetTransactionMock);

        leadsFacadeMock.BeginTransactionAsync(Any<CancellationToken>())
            .Returns(leadsTransactionMock);

        return (
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            intranetTransactionMock,
            leadsTransactionMock);
    }

    /// <summary>
    ///     Additionally to create & configure essentials, sets some data to DbSets. At least empty non-null list is
    ///     guaranteed.
    /// </summary>
    /// <returns></returns>
    public static (IApplicationDbContext, StubEntityClonerService<IApplicationDbContext>, ILeadsDbContext)
        BuildContextMocks(
            List<Contact>? knownContacts,
            List<Lead>? knownLeads = null,
            List<Action>? knownActions = null,
            List<Process>? knownProcesses = null,
            List<Annotation>? knownAnnotations = null,
            List<ContactLead>? knownContactLeads = null,
            List<ContactEmail>? knownContactEmails = null,
            List<ContactPhone>? knownContactPhones = null,
            List<ContactTitle>? knownContactTitles = null,
            List<OrdersImported>? knownOrdersImported = null,
            List<ContactAddress>? knownContactAddresses = null,
            List<ContactFaculty>? knownContactFaculties = null,
            List<ContactLanguage>? knownContactLanguages = null,
            List<ContactSpeciality>? knownContactSpecialities = null,
            List<ContactLeadProcess>? knownContactLeadProcesses = null
        )
    {
        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks();

        var leadsDbSetMock = (knownLeads ?? new()).BuildMockDbSet();
        leadsDbContextMock.Leads.Returns(leadsDbSetMock);

        // Build DbSet mocks: guarantees at least empty list, if method caller don't provide any known data.
        var actionsDbSetMock = (knownActions ??= new()).BuildMockDbSet();
        var contactDbSetMock = (knownContacts ??= new()).BuildMockDbSet();
        var processesDbSetMock = (knownProcesses ??= new()).BuildMockDbSet();
        var annotationsDbSetMock = (knownAnnotations ??= new()).BuildMockDbSet();
        var contactLeadsDbSetMock = (knownContactLeads ??= new()).BuildMockDbSet();
        var contactEmailsDbSetMock = (knownContactEmails ??= new()).BuildMockDbSet();
        var contactPhonesDbSetMock = (knownContactPhones ??= new()).BuildMockDbSet();
        var contactTitlesDbSetMock = (knownContactTitles ??= new()).BuildMockDbSet();
        var ordersImportedDbSetMock = (knownOrdersImported ??= new()).BuildMockDbSet();
        var contactAddressesDbSetMock = (knownContactAddresses ??= new()).BuildMockDbSet();
        var contactFacultiesDbSetMock = (knownContactFaculties ??= new()).BuildMockDbSet();
        var contactLanguagesDbSetMock = (knownContactLanguages ??= new()).BuildMockDbSet();
        var contactLeadProcessDbSetMock = (knownContactLeadProcesses ??= new()).BuildMockDbSet();
        var contactSpecialitiesDbSetMock = (knownContactSpecialities ??= new()).BuildMockDbSet();

        // Mock DbSet<>.Add(): calling it adds an entity into underlying in-memory list behind mock.
        actionsDbSetMock.Add(Do<Action>(e => knownActions.Add(e)));
        contactDbSetMock.Add(Do<Contact>(e => knownContacts.Add(e)));
        processesDbSetMock.Add(Do<Process>(e => knownProcesses.Add(e)));
        annotationsDbSetMock.Add(Do<Annotation>(e => knownAnnotations.Add(e)));
        contactLeadsDbSetMock.Add(Do<ContactLead>(e => knownContactLeads.Add(e)));
        contactEmailsDbSetMock.Add(Do<ContactEmail>(e => knownContactEmails.Add(e)));
        contactPhonesDbSetMock.Add(Do<ContactPhone>(e => knownContactPhones.Add(e)));
        contactTitlesDbSetMock.Add(Do<ContactTitle>(e => knownContactTitles.Add(e)));
        ordersImportedDbSetMock.Add(Do<OrdersImported>(e => knownOrdersImported.Add(e)));
        contactAddressesDbSetMock.Add(Do<ContactAddress>(e => knownContactAddresses.Add(e)));
        contactFacultiesDbSetMock.Add(Do<ContactFaculty>(e => knownContactFaculties.Add(e)));
        contactLanguagesDbSetMock.Add(Do<ContactLanguage>(e => knownContactLanguages.Add(e)));
        contactLeadProcessDbSetMock.Add(Do<ContactLeadProcess>(e => knownContactLeadProcesses.Add(e)));
        contactSpecialitiesDbSetMock.Add(Do<ContactSpeciality>(e => knownContactSpecialities.Add(e)));

        // Mock named DbSet<> properties' getters.
        intranetDbContextMock.Contact.Returns(contactDbSetMock);
        intranetDbContextMock.Actions.Returns(actionsDbSetMock);
        intranetDbContextMock.Processes.Returns(processesDbSetMock);
        intranetDbContextMock.Annotations.Returns(annotationsDbSetMock);
        intranetDbContextMock.ContactLeads.Returns(contactLeadsDbSetMock);
        intranetDbContextMock.ContactEmail.Returns(contactEmailsDbSetMock);
        intranetDbContextMock.ContactPhone.Returns(contactPhonesDbSetMock);
        intranetDbContextMock.ContactTitles.Returns(contactTitlesDbSetMock);
        intranetDbContextMock.OrdersImported.Returns(ordersImportedDbSetMock);
        intranetDbContextMock.ContactAddress.Returns(contactAddressesDbSetMock);
        intranetDbContextMock.ContactFaculty.Returns(contactFacultiesDbSetMock);
        intranetDbContextMock.ContactLanguages.Returns(contactLanguagesDbSetMock);
        intranetDbContextMock.ContactSpeciality.Returns(contactSpecialitiesDbSetMock);
        intranetDbContextMock.ContactLeadProcesses.Returns(contactLeadProcessDbSetMock);

        // Mock DbContext.Set<>(): used by stub that mimics cloning entities and adding them directly to ChangeTracker
        var intranetAsDbContextMock = (intranetDbContextMock as IUnitOfWork<IApplicationDbContext>)!;
        intranetAsDbContextMock.Set<Action>().Returns(actionsDbSetMock);
        intranetAsDbContextMock.Set<Contact>().Returns(contactDbSetMock);
        intranetAsDbContextMock.Set<Process>().Returns(processesDbSetMock);
        intranetAsDbContextMock.Set<Annotation>().Returns(annotationsDbSetMock);
        intranetAsDbContextMock.Set<ContactLead>().Returns(contactLeadsDbSetMock);
        intranetAsDbContextMock.Set<ContactEmail>().Returns(contactEmailsDbSetMock);
        intranetAsDbContextMock.Set<ContactPhone>().Returns(contactPhonesDbSetMock);
        intranetAsDbContextMock.Set<ContactTitle>().Returns(contactTitlesDbSetMock);
        intranetAsDbContextMock.Set<OrdersImported>().Returns(ordersImportedDbSetMock);
        intranetAsDbContextMock.Set<ContactAddress>().Returns(contactAddressesDbSetMock);
        intranetAsDbContextMock.Set<ContactFaculty>().Returns(contactFacultiesDbSetMock);
        intranetAsDbContextMock.Set<ContactLanguage>().Returns(contactLanguagesDbSetMock);
        intranetAsDbContextMock.Set<ContactSpeciality>().Returns(contactSpecialitiesDbSetMock);
        intranetAsDbContextMock.Set<ContactLeadProcess>().Returns(contactLeadProcessDbSetMock);

        return (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock);
    }

    public static void AssertExpectedLeadNotFoundError<TVal>(ErrorOr<TVal> errorOr) =>
        Assert.True(errorOr is
        {
            IsError: true,
            FirstError.NumericType: IContactsConsolidatorService.OriginLeadsNotFoundError,
        });
}
