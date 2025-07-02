using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

public sealed class AnnotationFaker : BaseFaker<Annotation>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public AnnotationFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<Annotation> root) =>
        root.RuleSet(WithContactRuleSet, set =>
        {
            var contactFaker = new ContactFaker(Locale, localSeed);

            set.RuleFor(a => a.Contact, () => contactFaker.Generate())
                .FinishWith((_, a) => a.ContactId = a.Contact.Id);
        });
}
