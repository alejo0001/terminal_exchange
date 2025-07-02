using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Employees.Queries.GetSignatureQuery;

[Authorize]
public readonly record struct GetSignatureQuery(string CorporateEmail) : IRequest<string>;

[UsedImplicitly]
public class GetSignatureQueryHandler : IRequestHandler<GetSignatureQuery, string>
{
    private const string ExcmEs = "Tech Universidad";
    private const int SignatureFiledId = 12;
    private readonly IApplicationDbContext _context;

    public GetSignatureQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<string> Handle(GetSignatureQuery request, CancellationToken cancellationToken)
    {
        var name = await _context.Employees
            .Where(e => e.CorporateEmail == request.CorporateEmail)
            .SelectMany(e => e.EmployeeAzureGroups)
            .Where(eag => eag.AzureGroup.IsSignatureGroup)
            .Where(eag => !eag.IsDeleted)
            .Select(eag => eag.AzureGroup.Name)
            .Distinct()
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(name))
        {
            return await GetQueryCatalogValue(name, cancellationToken);
        }

        name = await _context.Employees
            .Where(e => e.CorporateEmail == request.CorporateEmail)
            .SelectMany(e => e.CurrentOrganizationNode.OrganizationNodeAzureGroups)
            .Where(onag => onag.AzureGroup.IsSignatureGroup)
            .Where(onag => !onag.IsDeleted)
            .Select(onag => onag.AzureGroup.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(name))
        {
            return await GetQueryCatalogValue(name, cancellationToken);
        }

        return ExcmEs;
    }

    private async Task<string> GetQueryCatalogValue(
        string? name,
        CancellationToken cancellationToken)
    {
        var queryCatalog = await _context.CrmCatalogTableFieldValues
            .Where(v => v.ObjectName == name)
            .Where(v => v.CrmCatalogTableFieldId == SignatureFiledId)
            .Select(v => v.ObjectFieldValue)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(queryCatalog)
            ? ExcmEs
            : queryCatalog;
    }
}
