using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseFacultySyncDto : IMapFrom<CourseArea>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public int FacultyId { get; set; }
    public int CourseId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CourseArea, CourseFacultySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate))
            .ForMember(d => d.FacultyId, opt =>
                opt.MapFrom(dom => dom.AreaId));
        profile.CreateMap<CourseFacultySyncDto, CourseFaculty>();
    }
}
