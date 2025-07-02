using Bogus;

namespace Testing.Common.Fakers;

/// <summary>
///     General purpose base faker to apply strongly typed configurability that supports optionally related data pattern.
/// </summary>
/// <remarks>Id generation uses convention: starting from 1 and increments by 1.</remarks>
public abstract class BaseFaker<TModel> : Faker<TModel>
    where TModel : class
{
    protected int NextId;

    /// <summary>
    ///     If params are provided, then they will be passed down to object graph creation logic.
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="seed"></param>
    protected BaseFaker(string? locale, int? seed)
    {
        NextId = 1;

        if (!string.IsNullOrWhiteSpace(locale))
        {
            Locale = locale;
        }

        var root = seed is null
            ? this
            : UseSeed(seed.Value);

        Configure(root);
    }

    protected abstract void Configure(Faker<TModel> root);

    protected virtual int GetNextId() => NextId++;
}
