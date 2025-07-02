using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class CourseSyncDto : IMapFrom<Course>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public int OriginalCourseId { get; set; }
    public int? CourseTypeId { get; set; }
    public int? GuarantorId { get; set; }
    public float? Hours { get; set; }

    public DateTime? StartDate { get; set; }

    public bool IsActive { get; set; }
    public bool ActiveTlmk { get; set; }
    public bool ActiveWeb { get; set; }
    public bool Published { get; set; }

    public string Title { get; set; }
    public string OfficialTitle { get; set; }
    public string Code { get; set; }
    public string ParentCode { get; set; }
    public string BannerImageUrl { get; set; }
    public string VideoUrl { get; set; }

    public string SeoTitle { get; set; }
    public string SeoKeywords { get; set; }
    public string SeoUrl { get; set; }
    public string SeoDescription { get; set; }
    public string SeoContent { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Course, CourseSyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate));
        profile.CreateMap<CourseSyncDto, IntranetMigrator.Domain.Entities.Course>();
    }
}
