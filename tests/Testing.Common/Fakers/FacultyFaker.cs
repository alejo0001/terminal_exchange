using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class FacultyFaker : BaseFaker<Faculty>
{
    /// <inheritdoc />
    public FacultyFaker(string? locale, int? seed) : base(locale, seed) { }

    protected override void Configure(Faker<Faculty> root) =>
        root.RuleFor(c => c.Id, GetNextId)
            .RuleFor(c => c.Label, f => f.Name.JobTitle());
}
