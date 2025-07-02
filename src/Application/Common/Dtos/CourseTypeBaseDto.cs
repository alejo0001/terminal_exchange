using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseTypeBaseDto : IMapFrom<CourseTypeBase>
{
    public int Id { get; set; }
    public int OriginalCourseTypeId { get; set; }

    public string? Name { get; set; }

    public string? Label { get; set; }

    public string? SeoUrl { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoKeywords { get; set; }

    public string? SeoDescription { get; set; }

    public int Order { get; set; }

    public string? Code { get; set; }

    public bool CanBeParent { get; set; }

    public bool IsActive { get; set; }

    public bool IsPractical { get; set; }

    public string? GuarantorType { get; set; }

    public string? Modality { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CourseTypeBase, CourseTypeBaseDto>();
    }
}