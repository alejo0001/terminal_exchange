using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class FacultySpecialitySyncDto : IMapFrom<AreaSpeciality>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public string SeoTitle { get; set; }
    public string SeoDescription { get; set; }
    public string SeoKeywords { get; set; }
    public string SeoUrl { get; set; }
    public string SeoContent { get; set; }
    public int FacultyId { get; set; }
    public int SpecialityId { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<AreaSpeciality, FacultySpecialitySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate))
            .ForMember(d => d.SpecialityId, opt =>
                opt.MapFrom(dom => dom.SpecialityId))
            .ForMember(d => d.FacultyId, opt =>
                opt.MapFrom(dom => dom.AreaId));

        profile.CreateMap<FacultySpecialitySyncDto, FacultySpeciality>();
    }
}