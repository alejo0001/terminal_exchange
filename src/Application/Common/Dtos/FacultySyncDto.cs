using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class FacultySyncDto : IMapFrom<Area>
{
    public int Id { get; set; }

    public DateTime Created { get; set; }

    public DateTime LastModified { get; set; }

    public bool IsDeleted { get; set; }

    public string Name { get; set; }

    public string Label { get; set; }

    public string SeoUrl { get; set; }

    public string Video { get; set; }

    public string SeoTitle { get; set; }

    public string SeoKeywords { get; set; }

    public string SeoDescription { get; set; }

    public string SeoContent { get; set; }

    public int? Order { get; set; }

    public string Icon { get; set; }

    public string BackgroundImage { get; set; }

    public string Color { get; set; }

    public string Logo { get; set; }

    public string InverseLogo { get; set; }

    public string Code { get; set; }

    public bool? IsPlural { get; set; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Area, FacultySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate));

        profile.CreateMap<FacultySyncDto, Faculty>();
    }
}
