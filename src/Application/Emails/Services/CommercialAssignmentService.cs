using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Emails.Services;

public class CommercialAssignmentService : ICommercialAssignmentService
{
    private readonly IApplicationDbContext _context;
    
    private readonly ConcurrentDictionary<int, OrganizationNode> _organizationNodesCache;
    
    public CommercialAssignmentService(IApplicationDbContext context)
    {
        _context = context;
        _organizationNodesCache = new();
    }

    public async Task<ContactLead?> GetContactLead(int contactId, CancellationToken ct) =>
        await _context.ContactLeads
            .Where(cle => cle.ContactId == contactId)
            .Where(cle => !cle.IsDeleted)
            .Include(cl => cl.CourseCountry)
            .Include(cl => cl.Faculty)
            .FirstOrDefaultAsync(ct);
    
    //Eliminar esta ñapa ASAP
    private Employee? GetManagerRecursive(
        ConcurrentDictionary<int, OrganizationNode> allNodes,
        OrganizationNode? bottomNode,
        int employeeId = 0)
    {
        if (bottomNode is null)
        {
            return null;
        }

        var manager = bottomNode.Manager?.Id != employeeId
            ? bottomNode.Manager
            : null;

        return manager
               ?? GetManagerRecursive(
                   allNodes,
                   allNodes[bottomNode.OrganizationNodeParentId.Value],
                   employeeId);
    }
    
    public string? GetReceiver(Process process) =>
        process.Contact.ContactEmail?
            .Where(ce => !ce.IsDeleted)
            .MaxBy(ce => ce.IsDefault)
            ?.Email;
    
    /// <summary>
    ///     NB! Needs an explicit call to <see cref="ICommercialAssignmentService.FillOrganizationNodeCache" />
    ///     to have workable data.
    /// </summary>
    /// <param name="employee"></param>
    /// <returns>
    ///     Manager or <c>null</c>, if it cannot be found for any reason, starting from missing pre-cachedOrganization
    ///     Nodes!
    /// </returns>
    public ManagerDto? GetEmployeeManager(Employee employee)
    {
        if (employee.CurrentOrganizationNodeId is not { } currentOrganizationNodeId)
        {
            return null;
        }

        if (!_organizationNodesCache.TryGetValue(currentOrganizationNodeId, out var currentOrgNode))
        {
            return null;
        }

        if (GetManagerRecursive(_organizationNodesCache, currentOrgNode, employee.Id) is not { } manager)
        {
            return null;
        }

        return new()
        {
            givenName = manager.User.Name,
            surname = manager.User.Surname,
            corporateEmail = manager.CorporateEmail,
            corporatePhone = manager.CorporatePhone,
        };
    }
    
    public async Task FillOrganizationNodeCache(CancellationToken ct)
    {
        var allNodes = await _context.OrganizationNodes
            .AsNoTracking()
            .Include(on => on.Manager.User)
            .ToListAsync(ct);

        allNodes.ForEach(n => _organizationNodesCache.TryAdd(n.Id, n));
    }

}
