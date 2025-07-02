using System.Text;
using Bogus;
using CrmAPI.Domain.Leads.Entities;

namespace Testing.Common.Fakers;

public sealed class PotencialesLeadFaker : BaseFaker<Lead>
{
    public const string WithContactRuleSet = "WithContactRuleSet";

    /// <inheritdoc />
    public PotencialesLeadFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<Lead> root) =>
        root.RuleSet(
            WithContactRuleSet, set =>
            {
                var contactFaker = new ContactFaker(Locale, localSeed);

                set.RuleFor(
                    l => l.email,
                    () =>
                    {
                        var contact = contactFaker.Generate($"default,{ContactFaker.With2EmailsRuleSet}");

                        var sb = new StringBuilder();
                        sb.AppendJoin(',', contact.ContactEmail.Select(ce => ce.Email));

                        return sb.ToString();
                    });
            });
}
