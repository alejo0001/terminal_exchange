using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Infrastructure.Services;

public class OrganizationNodeExplorerService: IOrganizationNodeExplorerService
{
        
    private readonly IApplicationDbContext _context;
        
    public OrganizationNodeExplorerService(IApplicationDbContext context)
    {
        _context = context;
    }
        
        
    public async Task<int> GetTagIdNode(OrganizationNode node, string tagLabel)
    {
        var allNodes = await _context.OrganizationNodes
            .Include(on => on.OrganizationNodeTags)
            .ThenInclude(ont => ont.Tag)
            .Where(on => !on.IsDeleted)
            .Where(ont => !ont.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var organizationNodeTag = GetFirstNodeWithCrmTagRecursive(allNodes, node, tagLabel);

        if (organizationNodeTag == null)
        {
            return 0;
        }

        return organizationNodeTag.TagId;
    }


    private static OrganizationNodeTag GetFirstNodeWithCrmTagRecursive(List<OrganizationNode> allNodes,
        OrganizationNode bottomNode, string tagLabel)
    {
        if (bottomNode == null)
            return null;
            
        var organizationNodeTag = bottomNode.OrganizationNodeTags
            .Find(o => o.Tag.Label.Equals(tagLabel));
            
        if (organizationNodeTag == null)
            return GetFirstNodeWithCrmTagRecursive( allNodes, 
                allNodes.Find(o => o.Id == bottomNode.OrganizationNodeParentId), tagLabel);

        return organizationNodeTag;
    }
}