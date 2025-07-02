using System.Linq.Expressions;
using System.Text.RegularExpressions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Contacts.Services;
using CrmAPI.Domain.Leads.Entities;
using LinqKit;
using NSubstitute;
using Testing.Common.Fakers;
using Xunit;
using static Application.UnitTests.Contacts.Services.ContactsConsolidatorServiceTest.Helpers;
using static NSubstitute.Arg;
using static NSubstitute.Substitute;

namespace Application.UnitTests.Contacts.Services.ContactsConsolidatorServiceTest;

[TestSubject(typeof(ContactsConsolidatorService))]
public partial class ConsolidatePotencialesDataTests
{
    private const int RelatedDataSeed = 313;
    private const string Locale = "es";

    private static readonly IEFCoreFunctions EFCoreFunctionsMock = For<IEFCoreFunctions>();
    private readonly ContactFaker _contactFaker = new(Locale, RelatedDataSeed);
    private readonly PotencialesLeadFaker _leadFaker = new(Locale, RelatedDataSeed);
    private readonly Microsoft.Extensions.Logging.ILogger<ContactsConsolidatorService> _loggerMock = 
        For<Microsoft.Extensions.Logging.ILogger<ContactsConsolidatorService>>();
    
    [Fact(Skip = "Needs mocking update")]
    public async Task ConsolidateContacts_GetLeadsOfOriginContact_CreatedCorrectPredicate_Success()
    {
        // Arrange.
        var originContact = _contactFaker.Generate(DefaultWithRuleset(ContactFaker.With2EmailsRuleSet));
        var originLead = _leadFaker.Generate(DefaultWithRuleset(PotencialesLeadFaker.WithContactRuleSet));

        var originContactId = originContact.Id;

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks(
            new() { originContact },
            knownContactEmails: originContact.ContactEmail,
            knownLeads: new() { originLead }
        );

        const bool toHaveDeterministicError = false;
        EFCoreFunctionsMock.LikeOr(Any<Expression<Func<Lead, string>>>(), Any<string[]>())
            .Returns(lead => toHaveDeterministicError);

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(originContactId, default, default);

        // Assert.
        Assert.Multiple(
            () => AssertExpectedLeadNotFoundError(result),
            () => EFCoreFunctionsMock.Received().LikeOr(
                Is<Expression<Func<Lead, string>>>(exp => exp.Invoke(originLead) == originLead.email),
                Any<string[]>())
        );
    }

    [Fact(Skip = "Needs mocking update")]
    public async Task ConsolidateContacts_GetLeadsOfOriginContact_CreatedCorrectLikePatterns_Success()
    {
        // Arrange.
        var originContact = _contactFaker.Generate(
            DefaultWithRuleset(ContactFaker.With2EmailsRuleSet));

        var originContactId = originContact.Id;

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks(
            new() { originContact },
            knownContactEmails: originContact.ContactEmail
        );

        const bool toHaveDeterministicError = false;
        EFCoreFunctionsMock.LikeOr(Any<Expression<Func<Lead, string>>>(), Any<string[]>())
            .Returns(lead => toHaveDeterministicError);

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(originContactId, default, default);

        // Assert.
        Assert.Multiple(
            () => AssertExpectedLeadNotFoundError(result),
            () => EFCoreFunctionsMock.Received().LikeOr(
                Any<Expression<Func<Lead, string>>>(),
                Is<string[]>(orPatterns =>
                    orPatterns.Any() && orPatterns.All(p => LikeWildcardRegex().IsMatch(p))))
        );
    }

    [Fact(Skip = "Needs mocking update")]
    public async Task ConsolidateContacts_SetFusedTagForOriginLeads_CreatedCorrectTag_Success()
    {
        // Arrange.
        var expectedTag = string.Format(ContactsConsolidatorService.LeadTagFusionFormat, DateTime.UtcNow);

        var originContact = _contactFaker.Generate(
            DefaultWithRuleset(ContactFaker.With2EmailsRuleSet));

        var originContactId = originContact.Id;

        var contactEmails = originContact.ContactEmail
            .Select(ce => ce.Email)
            .ToList();

        var originLead1 = _leadFaker.Generate(DefaultWithRuleset(PotencialesLeadFaker.WithContactRuleSet));
        var originLead2 = _leadFaker.Generate(DefaultWithRuleset(PotencialesLeadFaker.WithContactRuleSet));

        // Ensure no existing tag
        originLead1.tags = string.Empty;
        // Make it of origin
        originLead2.email = originContact.ContactEmail[^1].Email;
        // Set duplicate tag scenario
        originLead2.tags = expectedTag;

        var originLeads = new List<Lead> { originLead1, originLead2 };

        var (intranetDbContextMock, stubEntityClonerService, leadsDbContextMock) = BuildContextMocks(
            new() { originContact },
            knownContactEmails: originContact.ContactEmail,
            knownLeads: originLeads
        );

        EFCoreFunctionsMock.LikeOr(Any<Expression<Func<Lead, string>>>(), Any<string[]>())
            .Returns(lead => true);

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            EFCoreFunctionsMock,
            _loggerMock);

        // Act.
        var result = await sut.ConsolidateContacts(originContactId, default, default);

        // Assert.
        Predicate<Lead> isOriginLeadPredicate = l => contactEmails.Any(
            ce => l.email!.Contains(ce, StringComparison.InvariantCultureIgnoreCase));

        Assert.Multiple(
            () => Assert.False(result.IsError),
            () => Assert.Equal(
                originLeads.Count,
                leadsDbContextMock.Leads.Count(isOriginLeadPredicate.Invoke)),
            () => Assert.All(
                leadsDbContextMock.Leads,
                // Verify that expected tag is present only once.
                l => Assert.Single(
                    l.tags!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                    t => t == expectedTag))
        );
    }

    private static string DefaultWithRuleset(string ruleSetName) => $"default,{ruleSetName}";

    [GeneratedRegex(@"%[\w\p{P}]+%")]
    private static partial Regex LikeWildcardRegex();
}
