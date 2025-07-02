using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AnnotationCreateDto : IMapFrom<Annotation>
{
    public int ContactId { get; set; }
    public string Comment { get; set; }
    public string Title { get; set; }
    public string LastEditor  { get; set; }
    public bool Mandatory  { get; set; }
    public bool IsPrivate  { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<AnnotationCreateDto, Annotation>().ReverseMap();
    }
}