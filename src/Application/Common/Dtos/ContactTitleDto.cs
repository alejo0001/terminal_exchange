using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactTitleDto : IMapFrom<ContactTitle>
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int TitleTypeId { get; set; }
    public string AcademicInstitution { get; set; }
    public string Degree { get; set; }
    public bool IsDeleted { get; set; }
    public TitleTypeDto? TitleType { get; set; }
}