using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Validators;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Common.Validation;

/// <summary>
///     Check whether Intranet DB has a Contact, that has specified email and both <see cref="Contact" /> and
///     <see cref="ContactEmail" /> are not deleted.
/// </summary>
/// <remarks>Reusable.</remarks>
/// <typeparam name="TModel"></typeparam>
[UsedImplicitly]
public sealed class ContactExistsByEmailValidator<TModel> : AsyncPropertyValidator<TModel, string>
{
    private readonly IApplicationDbContext _context;
    public ContactExistsByEmailValidator(IApplicationDbContext context) => _context = context;

    /// <inheritdoc />
    public override string Name => "ContactExistsByEmailValidator";

    /// <inheritdoc />
    protected override string GetDefaultMessageTemplate(string errorCode) =>
        "Contact for {PropertyName} '{PropertyValue}' was not found";

    /// <inheritdoc />
    public override async Task<bool> IsValidAsync(ValidationContext<TModel> _, string value, CancellationToken ct) =>
        await _context.Contact
            .Where(c => c.ContactEmail
                .Where(ce => ce.Email.ToUpper().Equals(value.Trim().ToUpper()))
                .Any(ce => !ce.IsDeleted))
            .Where(c => !c.IsDeleted)
            .AnyAsync(ct);
}
