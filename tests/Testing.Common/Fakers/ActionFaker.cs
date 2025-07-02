using Bogus;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace Testing.Common.Fakers;

public sealed class ActionFaker : BaseFaker<Action>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ActionFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<Action> root) =>
        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactFaker = new ContactFaker(Locale, localSeed);

            set.RuleFor(a => a.Contact, () => contactFaker.Generate())
                .FinishWith((_, a) => a.ContactId = a.Contact.Id);
        });
}
