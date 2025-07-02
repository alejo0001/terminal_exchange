using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ProcessFaker : BaseFaker<Process>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public ProcessFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<Process> root)
    {
        root.RuleFor(c => c.Id, GetNextId);

        root.RuleSet(
            WithContactRuleSet,
            set =>
            {
                var contactFaker = new ContactFaker(Locale, localSeed);

                set.RuleFor(c => c.Id, GetNextId);

                set.RuleFor(p => p.Contact, () => contactFaker.Generate())
                    .FinishWith((_, p) => p.ContactId = p.Contact.Id);
            });
    }
}
