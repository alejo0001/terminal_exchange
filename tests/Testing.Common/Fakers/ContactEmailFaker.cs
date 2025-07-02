using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ContactEmailFaker : BaseFaker<ContactEmail>
{
    /// <inheritdoc />
    public ContactEmailFaker(string? locale = null, int? seed = null) : base(locale, seed) { }

    protected override void Configure(Faker<ContactEmail> root) =>
        root.RuleFor(c => c.Id, GetNextId)
            .RuleFor(c => c.IsDefault, true)
            .RuleFor(c => c.Email, f => f.Person.Email);
}
