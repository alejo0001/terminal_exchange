using Bogus;
using IntranetMigrator.Domain.Entities;

namespace Testing.Common.Fakers;

/// <inheritdoc />
public sealed class SpecialityFaker : BaseFaker<Speciality>
{
    /// <inheritdoc />
    public SpecialityFaker(string? locale, int? seed) : base(locale, seed) { }

    /// <inheritdoc />
    protected override void Configure(Faker<Speciality> root) =>
        root.RuleFor(c => c.Id, GetNextId)
            .RuleFor(c => c.Label, f => f.Name.JobArea());
}
