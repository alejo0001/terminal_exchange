/*
using Bogus;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Emails.Services;
using CrmAPI.Application.Processes.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using IntranetMigrator.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NotificationAPI.Contracts.Commands;
using NSubstitute;
using Testing.Common.Fakers;
using Xunit;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace Application.UnitTests.Processes.Services;

[TestSubject(typeof(CrmMailerService))]
public class Records2ServiceTest
{
    private const string Locale = "es";

    private static readonly Faker Faker = new(Locale);
    private static readonly int RelatedDataSeed = Faker.Random.Number(50, 50_00);
    private static readonly ProcessFaker ProcessFaker = new(Locale, RelatedDataSeed);
    private static readonly CreateEmailFaker CreateEmailFaker = new(Locale, RelatedDataSeed);

    private readonly CrmMailerService _sut;
    private readonly IApplicationDbContext _applicationDbContextMock = Substitute.For<IApplicationDbContext>();

    public Records2ServiceTest()
    {
        var courseUnApiClient = Substitute.For<ICourseUnApiClient>();
        var dateTime = Substitute.For<IDateTime>();
        var blobStorageService = Substitute.For<IBlobStorageService>();
        var appSettingsService = Substitute.For<IAppSettingsService>();
        var managementApiClient = Substitute.For<IManagementApiClient>();
        var bus = Substitute.For<IBus>();
        var logger = Substitute.For<ILogger<CrmMailerService>>();

        _sut = new(
            _applicationDbContextMock,
            courseUnApiClient,
            bus,
            blobStorageService,
            appSettingsService,
            managementApiClient,
            logger);
    }

    private static (List<Process> processes, Dictionary<int, CreateEmail> processEmailCommands)
        GetCompleteProcessCreateEmailTestData(int processCount)
    {
        var processes = ProcessFaker.Generate(processCount);

        var processEmailCommands = processes.ToDictionary(
            p => p.Id,
            _ => CreateEmailFaker.Generate());

        return (processes, processEmailCommands);
    }

    public static TheoryData<IList<Process>, IDictionary<int, CreateEmail>> GeAllProcessesHavingEmails(int processCount)
    {
        var (processes, processEmailCommands) = GetCompleteProcessCreateEmailTestData(processCount);

        return new() { { processes, processEmailCommands } };
    }

    public static TheoryData<IList<Process>, IDictionary<int, CreateEmail>> GeSomeProcessesMissingEmails(
        int processCount)
    {
        var (processes, processEmailCommands) = GetCompleteProcessCreateEmailTestData(processCount);

        var randomIndex = Faker.Random.Int(0, processes.Count - 1);
        var processEmailToRemove = processes[randomIndex].Id;
        processEmailCommands.Remove(processEmailToRemove);

        return new() { { processes, processEmailCommands } };
    }

    [Theory]
    [MemberData(nameof(GeAllProcessesHavingEmails), 5)]
    [MemberData(nameof(GeSomeProcessesMissingEmails), 5)]
    public async Task StoreActionsAboutSentEmails_WithKnownProcessEmailCorrelations_ShouldCreateCorrespondingActions(
        IList<Process> processes,
        IDictionary<int, CreateEmail> processEmailCommands)
    {
        var actionsDbSetMock = new List<Action>().BuildMockDbSet();

        var createdActions = new List<Action>(processes.Count);
        actionsDbSetMock.Add(Arg.Do<Action>(a => createdActions.Add(a)));

        _applicationDbContextMock.Actions.Returns(actionsDbSetMock);

        // Act.
        await _sut.StoreActionsAboutSentEmails(processes, processEmailCommands, CancellationToken.None);

        // Assert.
        processEmailCommands.Should()
            .AllSatisfy(
                pe => createdActions.Should().Contain(a => a.ProcessId == pe.Key && a.Guid == pe.Value.CorrelationId));
    }

    [Fact]
    public async Task StoreActionsAboutSentEmails_WithKnownActions_ShouldBeSaved()
    {
        // Arrange.
        const int processEmailCount = 2;

        var (processes, processEmailCommands) = GetCompleteProcessCreateEmailTestData(processEmailCount);

        // Act.
        await _sut.StoreActionsAboutSentEmails(processes, processEmailCommands, CancellationToken.None);

        // Assert.
        using var _ = new AssertionScope();

        _applicationDbContextMock.Actions.Received(processEmailCount)
            .Add(Arg.Any<Action>());

        await _applicationDbContextMock.Received()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
*/
