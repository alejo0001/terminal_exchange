using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ContactFacultyFaker : BaseFaker<ContactFaculty>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ContactFacultyFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<ContactFaculty> root) =>
        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactFaker = new ContactFaker(Locale, localSeed);

            set.RuleFor(x => x.ContactId, () => contactFaker.Generate().Id);
        });
}
