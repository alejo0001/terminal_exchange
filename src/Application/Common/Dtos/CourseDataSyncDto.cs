using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class CourseDataSyncDto : IMapFrom<CourseData>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public int OriginalCourseId { get; set; }
    public int? CourseScheduleId { get; set; }
    public int CourseId { get; set; }
    public int CourseCountryId { get; set; }
    public int? DefaultCourseDataId { get; set; }
    public int? CourseSeoId { get; set; }
    public int? CourseCreditId { get; set; }
    public int? GuarantorId { get; set; }
    public float? Hours { get; set; }
    public float? Credits { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? PublicationDate { get; set; }
    public bool IsActive { get; set; }
    public string BannerImageUrl { get; set; }
    public string BoxImageUrl { get; set; }
    public float? ScholarshipAmount { get; set; }
    public decimal? EnrollmentPercent{get;set;}
    public float? TotalAmount { get; set; }
    public float? InitialFee { get; set; }
    public float? RecurrentFee { get; set; }
    public float? TotalPromoAmount { get; set; }
    public float? InitialPromoFee { get; set; }
    public float? RecurrentPromoFee { get; set; }
    public string Dossier { get; set; }
    public string VideoUrl { get; set; }
    public string Degree { get; set; }
    public int? FeesCount { get; set; }
    public float? AmountPercentage { get; set; }
    public string SeoTitle { get; set; }
    public string SeoKeywords { get; set; }
    public string SeoUrl { get; set; }
    public string SeoDescription { get; set; }
    public string SeoContent { get; set; }
    public string Title { get; set; }
    public string OfficialTitle { get; set; }
    public string Introduction { get; set; }
    public string Code { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CourseData, CourseDataSyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate))
            .ForMember(d => d.CourseCountryId, opt =>
                opt.MapFrom(dom => dom.CountryId));

        profile.CreateMap<CourseDataSyncDto, IntranetMigrator.Domain.Entities.CourseData>();
    }
}
