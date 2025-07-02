using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class CountryFacultySpecialitySyncDto : IMapFrom<CountryAreaSpeciality>
{

    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public int CourseCountryId { get; set; }
    public string SeoTitle { get; set; }
    public string SeoDescription { get; set; }
    public string SeoKeywords { get; set; }
    public string SeoUrl { get; set; }
    public string SeoContent { get; set; }
    public bool IsActive { get; set; }
    public int? FacultyId { get; set; }
    public int? SpecialityId { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CountryAreaSpeciality, CountryFacultySpecialitySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate))
            .ForMember(d => d.CourseCountryId, opt =>
                opt.MapFrom(dom => dom.CountryId));

        profile.CreateMap<CountryFacultySpecialitySyncDto, IntranetMigrator.Domain.Entities.CountryFacultySpeciality>();
    }
}