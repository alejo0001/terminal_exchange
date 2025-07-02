using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Contacts.Services;
using ErrorOr;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static Application.UnitTests.Contacts.Services.ContactsConsolidatorServiceTest.Helpers;
using static NSubstitute.Arg;
using static NSubstitute.Substitute;

namespace Application.UnitTests.Contacts.Services.ContactsConsolidatorServiceTest;

[TestSubject(typeof(ContactsConsolidatorService))]
public class SaveChangesManualTwoPhaseTransactionTests
{
    private static readonly IEFCoreFunctions StubEFCoreFunctions;
    private readonly Microsoft.Extensions.Logging.ILogger<ContactsConsolidatorService> _loggerMock = 
        For<Microsoft.Extensions.Logging.ILogger<ContactsConsolidatorService>>();
    
    static SaveChangesManualTwoPhaseTransactionTests() => StubEFCoreFunctions = For<IEFCoreFunctions>();

    [Fact]
    public async Task SaveChanges_TwoPhaseTransactionNoExceptions_AndCTSetupIsCorrect_Success()
    {
        // Arrange.
        var knownToken = new CancellationTokenSource().Token;

        var (intranetDbContextMock,
                stubEntityClonerService,
                leadsDbContextMock,
                intranetTransactionMock,
                leadsTransactionMock) =
            BuildContextAndTranMocks();

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            StubEFCoreFunctions,
            _loggerMock);

        // Act.
        var result = await sut.SaveChanges(knownToken);

        Assert.False(result.IsError);

        // We are keen to ensure that Saving changes have certain behavior, because it is manual Two-Phase Transaction.
        await intranetDbContextMock.Database.Received()
            .BeginTransactionAsync(Is<CancellationToken>(c => c == knownToken));

        await leadsDbContextMock.Database.Received()
            .BeginTransactionAsync(Is<CancellationToken>(c => c == knownToken));

        await intranetDbContextMock.Received()
            .SaveChangesAsync(Is<CancellationToken>(c => c == knownToken));

        await leadsDbContextMock.Received()
            .SaveChangesAsync(Is<CancellationToken>(c => c == knownToken));

        await intranetTransactionMock.Received()
            .CommitAsync(Is<CancellationToken>(c => c == knownToken));

        await leadsTransactionMock.Received()
            .CommitAsync(Is<CancellationToken>(c => c == knownToken));

        await intranetTransactionMock.DidNotReceiveWithAnyArgs()
            .RollbackAsync(Any<CancellationToken>());

        await leadsTransactionMock.DidNotReceiveWithAnyArgs()
            .RollbackAsync(Any<CancellationToken>());
    }

    /// <summary>
    ///     Provides asked number of datasets, every time mocks are created again.
    /// </summary>
    /// <returns>Mocks and order of dataset generated.</returns>
    public static TheoryData<
            IApplicationDbContext,
            StubEntityClonerService<IApplicationDbContext>,
            ILeadsDbContext,
            IDbContextTransaction,
            IDbContextTransaction, int>
        SaveChangesRollBackBehavioralTheories(int testCount)
    {
        TheoryData<
            IApplicationDbContext,
            StubEntityClonerService<IApplicationDbContext>,
            ILeadsDbContext,
            IDbContextTransaction,
            IDbContextTransaction, int> sets
            = new();

        // Create 4 test sets, with unique mocks every time.
        for (var i = 1; i <= testCount; i++)
        {
            // Create new mocks for every set.
            var (
                    intranetDbContextMock,
                    stubEntityClonerService,
                    leadsDbContextMock,
                    intranetTransactionMock,
                    leadsTransactionMock)
                = BuildContextAndTranMocks();

            sets.Add(
                intranetDbContextMock,
                stubEntityClonerService,
                leadsDbContextMock,
                intranetTransactionMock,
                leadsTransactionMock,
                i);
        }

        return sets;
    }

    [Theory]
    [MemberData(nameof(SaveChangesRollBackBehavioralTheories), 4)]
    public async Task SaveChanges_SavingTwoPhaseTransactionsThrows_AndCTSetupIsCorrect_Fails(
        IApplicationDbContext intranetDbContextMock,
        StubEntityClonerService<IApplicationDbContext> stubEntityClonerService,
        ILeadsDbContext leadsDbContextMock,
        IDbContextTransaction intranetTransactionMock,
        IDbContextTransaction leadsTransactionMock,
        int testSetOrderOf4)
    {
        // Arrange.
        var knownToken = new CancellationTokenSource().Token;

        // Setup exception throwing for a specific mock per dataset.
        switch (testSetOrderOf4)
        {
            case 1:
                intranetDbContextMock.SaveChangesAsync(Any<CancellationToken>())
                    .ThrowsAsyncForAnyArgs<Exception>();
                break;
            case 2:
                leadsDbContextMock.SaveChangesAsync(Any<CancellationToken>())
                    .ThrowsAsyncForAnyArgs<Exception>();
                break;
            case 3:
                intranetTransactionMock.CommitAsync(Any<CancellationToken>())
                    .ThrowsAsyncForAnyArgs<Exception>();
                break;
            case 4:
                leadsTransactionMock.CommitAsync(Any<CancellationToken>())
                    .ThrowsAsyncForAnyArgs<Exception>();
                break;
            default:
                throw new InvalidOperationException("Cannot have more than 4 test sets! This should not happen.");
        }

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            StubEFCoreFunctions,
            _loggerMock);

        // Act.
        var result = await sut.SaveChanges(knownToken);

        Assert.Multiple(
            () => Assert.True(result is
            {
                IsError: true,
                FirstError:
                {
                    Type: ErrorType.Failure,
                    Metadata.Keys.Count: > 0,
                },
            }),
            () => Assert.Contains(
                result.FirstError.Metadata!,
                pair => pair.Key.Equals("Exception") && pair.Value is Exception),
            // Verify, that CT passed to SaveChanges isn't used, because we don't allow to cancel the rollback.
            () => intranetTransactionMock.Received()
                .RollbackAsync(Is<CancellationToken>(c => c != knownToken)),
            () => leadsTransactionMock.Received()
                .RollbackAsync(Is<CancellationToken>(c => c != knownToken)));
    }

    [Theory]
    [MemberData(nameof(SaveChangesRollBackBehavioralTheories), 2)]
    public async Task SaveChanges_OneOfRollbackThrows_AndCTSetupIsCorrect_Fails(
        IApplicationDbContext intranetDbContextMock,
        StubEntityClonerService<IApplicationDbContext> stubEntityClonerService,
        ILeadsDbContext leadsDbContextMock,
        IDbContextTransaction intranetTransactionMock,
        IDbContextTransaction leadsTransactionMock,
        int testSetOrderOf2)
    {
        // Arrange.
        var knownToken = new CancellationTokenSource().Token;

        // Setup test scenario: throw Exception in saving phase.
        intranetDbContextMock.SaveChangesAsync(Any<CancellationToken>())
            .ThrowsAsyncForAnyArgs<Exception>();

        // Setup exception throwing for a specific mock per dataset.
        switch (testSetOrderOf2)
        {
            case 1:
                intranetTransactionMock.RollbackAsync(Any<CancellationToken>())
                    .ThrowsAsyncForAnyArgs<Exception>();
                break;
            case 2:
                leadsTransactionMock.RollbackAsync(Any<CancellationToken>())
                    .ThrowsAsyncForAnyArgs<Exception>();
                break;
            default:
                throw new InvalidOperationException("Cannot have more tha 2 test sets! This should not happen.");
        }

        var sut = new ContactsConsolidatorService(
            intranetDbContextMock,
            stubEntityClonerService,
            leadsDbContextMock,
            StubEFCoreFunctions,
            _loggerMock);

        // Act.
        var result = await sut.SaveChanges(knownToken);

        Assert.Multiple(
            () => Assert.True(result.IsError),
            () => Assert.Collection(
                result.Errors,
                e => Assert.True(e is
                {
                    Type: ErrorType.Failure,
                    Metadata.Keys.Count: > 0,
                }),
                e => Assert.True(e is
                {
                    Type: ErrorType.Unexpected,
                    Metadata.Keys.Count: > 0,
                })),
            // Verify, that CT passed to SaveChanges isn't used, because we don't allow to cancel therollback.
            () => intranetTransactionMock.Received()
                .RollbackAsync(Is<CancellationToken>(c => c != knownToken)),
            () => leadsTransactionMock.Received()
                .RollbackAsync(Is<CancellationToken>(c => c != knownToken)));
    }
}
