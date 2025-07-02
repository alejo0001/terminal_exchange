using System.Linq.Expressions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Contacts.Services;
using CrmAPI.Domain.Leads.Entities;
using IntranetMigrator.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Testing.Common.Fakers;
using Xunit;
using Action = IntranetMigrator.Domain.Entities.Action;
using static Application.UnitTests.Contacts.Services.ContactsConsolidatorServiceTest.Helpers;
using static NSubstitute.Arg;
using static NSubstitute.Substitute;

namespace Application.UnitTests.Contacts.Services.ContactsConsolidatorServiceTest;

[TestSubject(typeof(ContactsConsolidatorService))]
public class ConsolidateIntranetDataTests
{
    private const string Locale = "es";
    private const int RelatedDataSeed = 414;

    private static readonly IEFCoreFunctions EFCoreFunctionsMock;

    private readonly ActionFaker _actionFaker;
    private readonly ProcessFaker _processFaker;
    private readonly ContactFaker _contactFaker;
    private readonly AnnotationFaker _annotationFaker;
    private readonly ContactLeadFaker _contactLeadFaker;
    private readonly ContactTitleFaker _contactTitleFaker;
    private readonly ContactPhoneFaker _contactPhoneFaker;
    private readonly ContactAddressFaker _contactAddressFaker;
    private readonly ContactFacultyFaker _contactFacultyFaker;
    private readonly OrdersImportedFaker _ordersImportedFaker;
    private readonly ContactLanguageFaker _contactLanguageFaker;
    private readonly ContactSpecialityFaker _contactSpecialityFaker;
    private readonly ContactLeadProcessFaker _contactLeadProcessFaker;
    private readonly ILogger<ContactsConsolidatorService> _loggerMock = 
        Substitute.For<ILogger<ContactsConsolidatorService>>();

    public ConsolidateIntranetDataTests()
    {
        _actionFaker = new(Locale, RelatedDataSeed);
        _processFaker = new(Locale, RelatedDataSeed);
        _contactFaker = new(Locale, RelatedDataSeed);
        _annotationFaker = new(Locale, RelatedDataSeed);
        _contactLeadFaker = new(Locale, RelatedDataSeed);
        _contactTitleFaker = new(Locale, RelatedDataSeed);
        _contactPhoneFaker = new(Locale, RelatedDataSeed);
        _contactAddressFaker = new(Locale, RelatedDataSeed);
        _contactFacultyFaker = new(Locale, RelatedDataSeed);
        _ordersImportedFaker = new(Locale, RelatedDataSeed);
        _contactLanguageFaker = new(Locale, RelatedDataSeed);
        _contactSpecialityFaker = new(Locale, RelatedDataSeed);
        _contactLeadProcessFaker = new(Locale, RelatedDataSeed);
    }

    static ConsolidateIntranetDataTests()
    {
        EFCoreFunctionsMock = For<IEFCoreFunctions>();

        // This is controlled config, so that test scenarios exit from services' main method in deterministic way.
        EFCoreFunctionsMock.LikeOr(Any<Expression<Func<Lead, string>>>(), Any<string[]>())
            .Returns(l => false);
    }

    [Fact]
    public async Task ConsolidateContacts_GetOriginContactDto_NotFound_Fails()
    {
        // Arrange.
        var unknownContact = _contactFaker.Generate();
        var expectedContactId = unknownContact.Id + 10;

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) =
            BuildContextMocks(new() { unknownContact });

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(
            expectedContactId,
            default,
            default);

        // Assert.
        Assert.True(result is
        {
            IsError: true,
            FirstError.NumericType: IContactsConsolidatorService.OriginContactNotFoundError,
        });
    }

    [Fact(Skip = "Needs mocking update")]
    public async Task ConsolidateContacts_SetSoftDeleteOriginContactAndDesignatedRelatedEntities_Success()
    {
        // Arrange.
        var originContact = _contactFaker.Generate();
        var originContactId = originContact.Id;

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) =
            BuildConsolidationStep1TestCase(originContact);

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(
            originContact.Id,
            default,
            default);

        // Assert.
        Assert.Multiple(
            () => AssertExpectedLeadNotFoundError(result),
            () => Assert.True(originContact.IsDeleted),
            () => Assert.True(intranetDbContextMock.ContactLeads.Any(x => x.ContactId == originContactId)),
            () => Assert.True(intranetDbContextMock.ContactEmail.Any(x => x.ContactId == originContactId)),
            () => Assert.True(intranetDbContextMock.ContactPhone.Any(x => x.ContactId == originContactId)),
            () => Assert.True(intranetDbContextMock.ContactTitles.Any(x => x.ContactId == originContactId)),
            () => Assert.True(intranetDbContextMock.ContactAddress.Any(x => x.ContactId == originContactId)),
            () => Assert.True(intranetDbContextMock.ContactFaculty.Any(x => x.ContactId == originContactId)),
            () => Assert.True(intranetDbContextMock.ContactLanguages.Any(x => x.ContactId == originContactId)),
            () => Assert.True(intranetDbContextMock.ContactSpeciality.Any(x => x.ContactId == originContactId)),
            () => Assert.True(
                intranetDbContextMock.ContactLeadProcesses.Any(x => x.ContactLead.ContactId == originContactId)),
            () => Assert.All(
                intranetDbContextMock.ContactLeads.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactEmail.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactPhone.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactTitles.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactAddress.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactFaculty.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactLanguages.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactSpeciality.Where(x => x.ContactId == originContactId),
                x => Assert.True(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactLeadProcesses.Where(x => x.ContactLead.ContactId == originContactId),
                x => Assert.True(x.IsDeleted))
        );
    }

    [Fact(Skip = "Needs mocking update")]
    public async Task ConsolidateContacts_UpdateDesignatedRelatedEntitiesToDestinationContact_Success()
    {
        // Arrange.
        var originContact = _contactFaker.Generate();
        var destinationContactId = originContact.Id + 10;

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock)
            = BuildConsolidationStep2TestCase(originContact);

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(
            originContact.Id,
            destinationContactId,
            default);

        // Assert.
        Assert.Multiple(
            () => AssertExpectedLeadNotFoundError(result),
            () => Assert.True(intranetDbContextMock.Actions.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.Processes.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.Annotations.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.OrdersImported.Any(x => x.ContactId == destinationContactId)),
            () => Assert.All(
                intranetDbContextMock.Actions.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.Processes.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.Annotations.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.OrdersImported.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)));
    }

    [Fact(Skip = "Needs mocking update")]
    public async Task ConsolidateContacts_DeepCopyDesignatedRelatedEntitiesToDestinationContact_Success()
    {
        // Arrange.
        var originContact = _contactFaker.Generate();
        var destinationContactId = originContact.Id + 10;

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock)
            = BuildConsolidationStep2TestCase(originContact);

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(
            originContact.Id,
            destinationContactId,
            default);

        // Assert.
        Assert.Multiple(
            () => AssertExpectedLeadNotFoundError(result),
            () => Assert.True(intranetDbContextMock.ContactEmail.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.ContactPhone.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.ContactTitles.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.ContactAddress.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.ContactFaculty.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.ContactLanguages.Any(x => x.ContactId == destinationContactId)),
            () => Assert.True(intranetDbContextMock.ContactSpeciality.Any(x => x.ContactId == destinationContactId)),
            () => Assert.All(
                intranetDbContextMock.ContactEmail.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactPhone.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactTitles.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactAddress.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactFaculty.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactLanguages.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted)),
            () => Assert.All(
                intranetDbContextMock.ContactSpeciality.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted))
        );
    }

    [Fact]
    public async Task ConsolidateContacts_DeepCopySpecialCaseContactLeadToDestinationContact_Success()
    {
        // Arrange.
        var originContact = _contactFaker.Generate();
        var destinationContactId = originContact.Id + 10;

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock, expectedDestinationLeadCount)
            = BuildConsolidationStep3TestCase(originContact, destinationContactId);

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(
            originContact.Id,
            destinationContactId,
            default);

        // Assert.
        Assert.Multiple(
            () => AssertExpectedLeadNotFoundError(result),
            () => Assert.True(intranetDbContextMock.ContactLeads.Any(x => x.ContactId == destinationContactId)),
            () => Assert.Equal(
                expectedDestinationLeadCount,
                intranetDbContextMock.ContactLeads.Count(x => x.ContactId == destinationContactId)),
            () => Assert.All(
                intranetDbContextMock.ContactLeads.Where(x => x.ContactId == destinationContactId),
                x => Assert.False(x.IsDeleted))
        );
    }

    private (
        IApplicationDbContext,
        StubEntityClonerService<IApplicationDbContext>,
        ILeadsDbContext,
        int expectedDestinationLeadCount)
        BuildConsolidationStep3TestCase(Contact originContact, int destinationContactId)
    {
        var originContactLeadFaker = _contactLeadFaker
            .RuleFor(c => c.ContactId, originContact.Id)
            .RuleFor(c => c.CourseId, f => f.Random.Number(1_000, 9_999))
            .RuleFor(c => c.CourseCode, f => f.Random.AlphaNumeric(5));

        var relatedByDeterminismOriginContactLead1 = originContactLeadFaker.Generate();
        var relatedByDeterminismOriginContactLead2 = originContactLeadFaker.Generate();
        var relatedByDeterminismOriginContactLeadToBeCopied = originContactLeadFaker.Generate();

        var destinationContactLeadFaker = _contactLeadFaker
            .RuleFor(c => c.ContactId, destinationContactId);

        var relatedByDeterminismDestinationContactLeadMatchWithOrigin1 = destinationContactLeadFaker
            .RuleFor(c => c.CourseId, relatedByDeterminismOriginContactLead1.CourseId)
            .RuleFor(c => c.CourseCode, f => f.Random.AlphaNumeric(5))
            .Generate();

        var relatedByDeterminismDestinationContactLeadMatchWithOrigin2 = destinationContactLeadFaker
            .RuleFor(c => c.CourseId, f => f.Random.Number(1_000, 9_999))
            .RuleFor(c => c.CourseCode, relatedByDeterminismOriginContactLead2.CourseCode)
            .Generate();

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks(
            new() { originContact },
            knownContactLeads: new()
            {
                relatedByDeterminismOriginContactLead1,
                relatedByDeterminismOriginContactLead2,
                relatedByDeterminismOriginContactLeadToBeCopied,
                relatedByDeterminismDestinationContactLeadMatchWithOrigin1,
                relatedByDeterminismDestinationContactLeadMatchWithOrigin2,
            }
        );

        return (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock, 3);
    }

    private (IApplicationDbContext, StubEntityClonerService<IApplicationDbContext>, ILeadsDbContext)
        BuildConsolidationStep1TestCase(Contact originContact)
    {
        var (knownContactLead,
            knownContactTitle,
            knownContactPhone,
            knownContactAddress,
            knownContactLanguage,
            knownContactFaculty,
            knownContactSpeciality,
            knownContactLeadProcess) = GetConsolidationStep1TestData();

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks(
            new() { originContact },
            knownContactLeads: new() { knownContactLead },
            knownContactTitles: new() { knownContactTitle },
            knownContactEmails: originContact.ContactEmail,
            knownContactPhones: new() { knownContactPhone },
            knownContactAddresses: new() { knownContactAddress },
            knownContactLanguages: new() { knownContactLanguage },
            knownContactFaculties: new() { knownContactFaculty },
            knownContactSpecialities: new() { knownContactSpeciality },
            knownContactLeadProcesses: new() { knownContactLeadProcess }
        );

        return (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock);
    }

    private (
        ContactLead,
        ContactTitle,
        ContactPhone,
        ContactAddress,
        ContactLanguage,
        ContactFaculty,
        ContactSpeciality,
        ContactLeadProcess )
        GetConsolidationStep1TestData()
    {
        var knownContactLeads = _contactLeadFaker.Generate(
            DefaultWithRuleset(ContactLeadFaker.WithContactRuleSet));

        var knownContactTitle = _contactTitleFaker.Generate(
            DefaultWithRuleset(ContactTitleFaker.WithContactRuleSet));

        var knownContactPhone = _contactPhoneFaker.Generate(
            DefaultWithRuleset(ContactPhoneFaker.WithContactRuleSet));

        var knownContactAddress = _contactAddressFaker.Generate(
            DefaultWithRuleset(ContactAddressFaker.WithContactRuleSet));

        var knownContactLanguage = _contactLanguageFaker.Generate(
            DefaultWithRuleset(ContactLanguageFaker.WithContactRuleSet));

        var knownContactFaculty = _contactFacultyFaker.Generate(
            DefaultWithRuleset(ContactFacultyFaker.WithContactRuleSet));

        var knownContactSpeciality = _contactSpecialityFaker.Generate(
            DefaultWithRuleset(ContactSpecialityFaker.WithContactRuleSet));

        var knownContactLeadProcess
            = _contactLeadProcessFaker.Generate(DefaultWithRuleset(ContactLeadProcessFaker.WithContactRuleSet));

        return (
            knownContactLeads,
            knownContactTitle,
            knownContactPhone,
            knownContactAddress,
            knownContactLanguage,
            knownContactFaculty,
            knownContactSpeciality,
            knownContactLeadProcess);
    }

    private (IApplicationDbContext, StubEntityClonerService<IApplicationDbContext>, ILeadsDbContext)
        BuildConsolidationStep2TestCase(Contact originContact)
    {
        var (knownContactLead,
            knownContactTitle,
            knownContactPhone,
            knownContactAddress,
            knownContactLanguage,
            knownContactFaculty,
            knownContactSpeciality,
            knownContactLeadProcess) = GetConsolidationStep1TestData();

        var (knownAction, knownProcess, knownAnnotation, knownOrdersImported) = GetConsolidationStep2TestData();

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks(
            new() { originContact },
            knownActions: new() { knownAction },
            knownProcesses: new() { knownProcess },
            knownAnnotations: new() { knownAnnotation },
            knownContactLeads: new() { knownContactLead },
            knownContactTitles: new() { knownContactTitle },
            knownContactEmails: originContact.ContactEmail,
            knownContactPhones: new() { knownContactPhone },
            knownOrdersImported: new() { knownOrdersImported },
            knownContactAddresses: new() { knownContactAddress },
            knownContactLanguages: new() { knownContactLanguage },
            knownContactFaculties: new() { knownContactFaculty },
            knownContactSpecialities: new() { knownContactSpeciality },
            knownContactLeadProcesses: new() { knownContactLeadProcess }
        );

        return (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock);
    }

    private (Action, Process, Annotation, OrdersImported ) GetConsolidationStep2TestData()
    {
        var knownAction = _actionFaker.Generate(DefaultWithRuleset(ActionFaker.WithContactRuleSet));

        var knownProcess = _processFaker.Generate(DefaultWithRuleset(ProcessFaker.WithContactRuleSet));

        var knownAnnotation = _annotationFaker.Generate(DefaultWithRuleset(AnnotationFaker.WithContactRuleSet));

        var knownOrdersImported = _ordersImportedFaker.Generate(
            DefaultWithRuleset(OrdersImportedFaker.WithContactRuleSet));

        return (
            knownAction,
            knownProcess,
            knownAnnotation,
            knownOrdersImported);
    }

    private static string DefaultWithRuleset(string ruleSetName) => $"default,{ruleSetName}";
}
