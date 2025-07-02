using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class FacultyDto : IMapFrom<Faculty>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }
    public string SeoUrl { get; set; }
    public string Color { get; set; }
    public string Code { get; set; }
}