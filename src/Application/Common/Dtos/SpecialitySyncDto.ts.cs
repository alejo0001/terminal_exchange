using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class SpecialitySyncDto : IMapFrom<Speciality>
{
    public int Id { get; set; }

    public DateTime Created { get; set; }

    public DateTime LastModified { get; set; }

    public bool IsDeleted { get; set; }

    public string Name { get; set; }

    public string Label { get; set; }

    public string SeoUrl { get; set; }

    public string SeoTitle { get; set; }

    public string SeoKeywords { get; set; }

    public string SeoDescription { get; set; }

    public string SeoContent { get; set; }

    public int? OriginalCategoryId { get; set; }

    public string Code { get; set; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Speciality, SpecialitySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate));

        profile.CreateMap<SpecialitySyncDto, IntranetMigrator.Domain.Entities.Speciality>();
    }
}
