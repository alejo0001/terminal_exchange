using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class SpecialityDto : IMapFrom<Speciality>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Label { get; set;}
    public string SeoUrl{ get; set; }
    public string SeoTitle { get; set; }
    public string OriginalCategoryId { get; set; }
    public string Code { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Speciality, SpecialityDto>();
    }
}