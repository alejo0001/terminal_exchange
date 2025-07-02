using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <summary>
///     Id generation uses convention: starting from 1 and increments by 1. <br /> <br />
///     <inheritdoc />
/// </summary>
public sealed class ContactLeadFaker : BaseFaker<ContactLead>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ContactLeadFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<ContactLead> root) =>
        root.RuleSet(
            WithContactRuleSet, set =>
            {
                var contactFaker = new ContactFaker(Locale, localSeed);

                set.RuleFor(cl => cl.Contact, () => contactFaker.Generate())
                    .FinishWith((_, cl) => cl.ContactId = cl.Contact.Id);
            });
}
