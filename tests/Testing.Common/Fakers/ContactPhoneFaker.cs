using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ContactPhoneFaker : BaseFaker<ContactPhone>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ContactPhoneFaker(string? locale, int? seed) : base(locale, seed) { }

    protected override void Configure(Faker<ContactPhone> root)
    {
        root.RuleFor(c => c.Id, GetNextId)
            .RuleFor(c => c.IsDefault, true)
            .RuleFor(c => c.Phone, f => f.Person.Phone);

        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactFaker = new ContactFaker(Locale, localSeed);

            set.RuleFor(cp => cp.Contact, () => contactFaker.Generate())
                .FinishWith((_, cp) => cp.ContactId = cp.Contact.Id);
        });
    }
}
