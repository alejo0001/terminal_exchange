using Bogus;
using NotificationAPI.Contracts.Commands;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class CreateEmailFaker : BaseFaker<CreateEmail>
{
    /// <inheritdoc />
    public CreateEmailFaker(string? locale = null, int? seed = null) : base(locale, seed) { }

    protected override void Configure(Faker<CreateEmail> root) => root.RuleFor(c => c.CorrelationId, Guid.NewGuid);
}
