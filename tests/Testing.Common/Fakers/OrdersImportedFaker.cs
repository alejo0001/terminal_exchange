using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

public sealed class OrdersImportedFaker : BaseFaker<OrdersImported>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public OrdersImportedFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<OrdersImported> root) =>
        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactFaker = new ContactFaker(Locale, localSeed);

            set.RuleFor(o => o.Contact, () => contactFaker.Generate())
                .FinishWith((_, o) => o.ContactId = o.Contact.Id);
        });
}
