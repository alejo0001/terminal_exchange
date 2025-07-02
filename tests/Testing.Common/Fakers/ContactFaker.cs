using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class ContactFaker : BaseFaker<Contact>
{
    public const string With2EmailsRuleSet = "With2EmailsRuleSet";
    private readonly ContactEmailFaker _contactEmailFaker;

    public ContactFaker(string? locale = null, int? seed = null) : base(locale, seed) =>
        _contactEmailFaker = new(locale, seed);

    protected override void Configure(Faker<Contact> root)
    {
        root.RuleFor(c => c.Id, GetNextId)
            .RuleFor(c => c.Email, f => f.Person.Email)
            .RuleFor(c => c.FirstSurName, f => f.Person.LastName)
            .RuleFor(c => c.SecondSurName, f => f.Name.LastName())
            .RuleFor(c => c.ContactEmail, () => _contactEmailFaker.Generate(1))
            .FinishWith((_, c) => c.ContactEmail.ForEach(e => e.ContactId = c.Id));

        root.RuleSet(
            With2EmailsRuleSet,
            set => set.RuleFor(c => c.ContactEmail, () => _contactEmailFaker.Generate(2)));
    }
}
