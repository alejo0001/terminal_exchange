using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AnnotationUpdateDto : IMapFrom<Annotation>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Comment { get; set; }
    public string Title { get; set; }
    public string LastEditor  { get; set; }
    public bool Mandatory { get; set; }
    public bool IsPrivate  { get; set; }
}