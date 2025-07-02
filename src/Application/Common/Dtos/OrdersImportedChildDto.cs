using System;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class OrdersImportedChildDto : IMapFrom<IntranetMigrator.Domain.Entities.OrdersImported>
{
    public int Id { get; set; }
    public int OrderNumber { get; set; }
    public int? CourseId { get; set; }
    public int? ProcessId { get; set; }
    public int? ActionId { get; set; }
    public int? ContactId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentSurName { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Title { get; set; }
    public string? Area { get; set; }
    public string? CourseCode { get; set; }

    public void Mapping(MappingProfile profile)
    {
        profile.CreateMap<IntranetMigrator.Domain.Entities.OrdersImported, OrdersImportedChildDto>();
    }
}