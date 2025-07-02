using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ContactSpecialityFaker : BaseFaker<ContactSpeciality>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ContactSpecialityFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<ContactSpeciality> root) =>
        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactFaker = new ContactFaker(Locale, localSeed);

            set.RuleFor(cs => cs.Contact, () => contactFaker.Generate())
                .FinishWith((_, cs) => cs.ContactId = cs.Contact.Id);
        });
}
