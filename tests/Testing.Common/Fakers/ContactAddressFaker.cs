using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ContactAddressFaker : BaseFaker<ContactAddress>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ContactAddressFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<ContactAddress> root) =>
        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactFaker = new ContactFaker(Locale, localSeed);

            set.RuleFor(ca => ca.Contact, () => contactFaker.Generate())
                .FinishWith((_, ca) => ca.ContactId = ca.Contact.Id);
        });
}
