namespace CrmAPI.Application.Common.Utils;

/// <summary>
///     Encapsulate some peculiar logic to generate values for DB fields like "CreatedBy", "ModifiedBy".
/// </summary>
/// <remarks>
///     To be used in DbContext Interceptors, in DbContext implementations' <c>SaveChanges*(...)</c> overrides; or DbSet's
///     <c>Execute*()</c> methods.
/// </remarks>
public static class PersistenceAuditingHelper
{
    public const string DefaultCreatedByActor = "System";

    /// <summary>
    ///     Depending on provided values produces one of the following outputs:<br />
    ///     - if e-mail by minimum validation of having `@` char is found => <c>username+tagValue@domain.com</c>
    ///     (it is supported by standard).<br />
    ///     - if email is string w/o `@` then provided value is appended like => <c>system:tagValue</c>.<br />
    ///     - if <paramref name="tag" /> is empty, then value of <paramref name="emailOrName" /> is returned <em>as is</em>.
    /// </summary>
    /// <param name="emailOrName"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static string InjectCreatorPartialAsTag(string? emailOrName, string? tag)
    {
        emailOrName = string.IsNullOrWhiteSpace(emailOrName)
            ? DefaultCreatedByActor
            : emailOrName.Trim();

        return string.IsNullOrWhiteSpace(tag)
            ? emailOrName
            : emailOrName.Split("@") is not { Length: 2 } emailParts
                ? $"{emailOrName}:{tag}"
                : $"{emailParts[0]}+{tag}@{emailParts[1]}";
    }
}
