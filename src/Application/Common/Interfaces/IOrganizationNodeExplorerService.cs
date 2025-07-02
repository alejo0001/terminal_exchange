using System.Threading.Tasks;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface IOrganizationNodeExplorerService
{
    public Task<int> GetTagIdNode(OrganizationNode node, string tagLabel);
}