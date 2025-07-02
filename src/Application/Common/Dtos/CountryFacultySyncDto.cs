using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CountryFacultySyncDto : IMapFrom<CountryArea>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }
    public string SeoTitle { get; set; }
    public string SeoDescription { get; set; }
    public string SeoKeywords { get; set; }
    public string SeoUrl { get; set; }
    public string SeoContent { get; set; }
    public bool IsActive { get; set; }
    public int FacultyId { get; set; }
    public int CourseCountryId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CountryArea, CountryFacultySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate))
            .ForMember(d => d.CourseCountryId, opt =>
                opt.MapFrom(dom => dom.CountryId))
            .ForMember(d => d.FacultyId, opt =>
                opt.MapFrom(dom => dom.AreaId));

        profile.CreateMap<CountryFacultySyncDto, CountryFaculty>();
    }
}
