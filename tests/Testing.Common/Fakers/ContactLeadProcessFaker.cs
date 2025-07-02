using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ContactLeadProcessFaker : BaseFaker<ContactLeadProcess>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ContactLeadProcessFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<ContactLeadProcess> root) =>
        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactLeadFaker = new ContactLeadFaker(Locale, localSeed);

            set.RuleFor(clp => clp.ContactLead, () => contactLeadFaker.Generate(ContactLeadFaker.WithContactRuleSet));
        });
}
