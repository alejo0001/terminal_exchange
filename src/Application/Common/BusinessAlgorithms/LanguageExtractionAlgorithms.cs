using System.Linq;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.BusinessAlgorithms;

/// <summary>
///     Various common ways to get Contact Language info from in-memory data structures, based on Crm Domain Model.
/// </summary>
public static class LanguageExtractionAlgorithms
{
    /// <summary>
    ///     By business rule, Spanish is default language throughout the company. It is the Id of
    ///     <see cref="Language" /> entity.
    /// </summary>
    public const int DefaultLanguageIdES = 1;

    public static (int Result, bool IsFallbackLanguageId) GetContactLanguageIdOrDefault(Process process)
    {
        var contactLanguage = process.Contact.ContactLanguages
            .Where(cl => !cl.IsDeleted)
            .MaxBy(cl => cl.IsDefault);

        if (contactLanguage?.LanguageId is { } langId)
        {
            return (langId, false);
        }

        return (DefaultLanguageIdES, true);
    }
}
