using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ContactCourseContactCreateDto : IMapFrom<ContactCourse>
{
    public int CourseId { get; set; }
    public string Type { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactCourseContactCreateDto, ContactCourse>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dto => Enum.Parse<ContactCourseType>(dto.Type, true)));
    }
}