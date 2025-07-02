using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using CourseSpeciality = CourseApi.Data.Domain.CourseSpeciality;

namespace CrmAPI.Application.Common.Dtos;

public class CourseSpecialitySyncDto : IMapFrom<CourseSpeciality>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public int CourseId { get; set; }
    public int SpecialityId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CourseSpeciality, CourseSpecialitySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate))
            .ForMember(d => d.SpecialityId, opt =>
                opt.MapFrom(dom => dom.SpecialityId));

        profile.CreateMap<CourseSpecialitySyncDto, IntranetMigrator.Domain.Entities.CourseSpeciality>();
    }
}
