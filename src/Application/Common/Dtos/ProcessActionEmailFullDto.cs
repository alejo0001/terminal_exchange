using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessActionEmailFullDto : IMapFrom<Process>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ContactId { get; set; }
    public int? OrdersImportedId { get; set; }
}