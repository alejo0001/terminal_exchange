using System.Linq;
using System.Text;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Utils;

/// <summary>
///     Common logic to get email headers data from known CRM Domain Model object graphs.
/// </summary>
/// <remarks>
///     It is up to the caller to provide sufficient object graph for each method, because it works on in-memory data.
/// </remarks>
public static class EmailHeadersHelper
{
    /// <summary>
    ///     Get From address from object graphs or default to <paramref name="fallBackAddress" />
    /// </summary>
    /// <param name="process"></param>
    /// <param name="fromEmail"></param>
    /// <param name="fallBackAddress"></param>
    /// <returns>Sanitized string or <see cref="string.Empty" /> if impossible to obtain.</returns>
    public static string GetFrom(Process process, string? fromEmail, string? fallBackAddress)
    {
        var result = string.IsNullOrWhiteSpace(fromEmail)
            ? string.IsNullOrWhiteSpace(process.User?.Employee?.CorporateEmail)
                ? fallBackAddress ?? string.Empty
                : process.User.Employee.CorporateEmail
            : fromEmail;

        return SanitizeEmailAddress(result);
    }

    /// <summary>
    ///     Uses <see cref="string.Normalize(NormalizationForm)" /> with argument
    ///     <see cref="NormalizationForm.FormKD" /> to try to get rid of fancy Unicode characters.
    ///     It will put string to lower-invariant and trim it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This converter could be made more precise given the examples of
    ///         <a href="https://en.wikipedia.org/wiki/Email_address#Examples">Valid email addresses</a>.
    ///     </para>
    ///     <para>
    ///         NB! It won't get rid of all kinds of Unicode chars! Therefor might need attention in future to improve
    ///         normalization process. From <a href="https://lingojam.com/FancyFontGenerator,UnicodeText">this link</a>
    ///         it is possible to generate various fancy strings to test limits of this value converter.
    ///     </para>
    /// </remarks>
    public static string SanitizeEmailAddress(string? source)
    {
        var result = source?.Normalize(NormalizationForm.FormKD)
            .Trim()
            .ToLowerInvariant();

        return string.IsNullOrWhiteSpace(result)
            ? string.Empty
            : result;
    }

    /// <summary>
    /// Function for obtain receiver in Process.Contact.ContactEmail
    /// </summary>
    /// <param name="process"></param>
    /// <returns>Return ContactEmail by default OR other contactEmail OR NULL</returns>
    public static string GetReceiver(Process process)
    {
        var result = process.Contact.ContactEmail.Where(e => !e.IsDeleted)
            ?.MaxBy(ce=>ce.IsDefault)?.Email 
                     ?? string.Empty;

        return SanitizeEmailAddress(result);
    }

    /// <summary>
    ///     Creates "From" displayed from Process object graph.
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    public static string GetFromName(Process process) =>
        $"{process.User.Name} {process.User.Surname} | {EmailContentHelper.TechInstitutionName}";
}
