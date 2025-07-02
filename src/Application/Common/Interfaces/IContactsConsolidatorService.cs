using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace CrmAPI.Application.Common.Interfaces;

/// <summary>
///     Consolidates 2 Contacts (student entity or sales lead): copies over sales and contact information, origin to
///     destination, origin gets soft deleted.<br />
///     <para>
///         Systems involved:<br />
///         1. Intranet DB<br />
///         2. "Potenciales" DB.
///     </para>
/// </summary>
/// <remarks>
///     It is required that all work must be transactional: in case of any problem all must be rolled back.
/// </remarks>
public interface IContactsConsolidatorService
{
    public const int OriginContactNotFoundError = -1;
    public const int OriginLeadsNotFoundError = -2;

    /// <summary>
    ///     Conducts consolidation from origin contact to destination, encapsulates entity changes only.
    /// </summary>
    /// <remarks>
    ///     Saving changes must be called outside to provide some degree of flexibility, data analysis and other kind of
    ///     control as needed, before saving modifications to the datastores.
    /// </remarks>
    /// <param name="originContactId"></param>
    /// <param name="destinationContactId"></param>
    /// <param name="ct"></param>
    /// <returns>
    ///     If Origin happens to be not found then returns <see cref="Error" /> of <see cref="ErrorType.NotFound" />
    /// </returns>
    Task<ErrorOr<Success>> ConsolidateContacts(int originContactId, int destinationContactId, CancellationToken ct);

    /// <summary>
    ///     Performs transactional saving changes of consolidation work in transactional over all underlying datastores.
    /// </summary>
    /// <remarks>Implementations must guarantee that nothing gets saved in cas of any error.</remarks>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<ErrorOr<Success>> SaveChanges(CancellationToken ct);
}
