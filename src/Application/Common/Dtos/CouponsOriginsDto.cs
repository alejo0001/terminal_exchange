using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CouponsOriginsDto : IMapFrom<CouponOrigin>
{
    public int Id { get; set; }
    public string Name { get; set; }
}