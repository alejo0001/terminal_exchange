using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class CourseDataGuarantorSyncDto : IMapFrom<CourseDataGuarantor>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public int CourseDataId { get; set; }
    public int GuarantorId { get; set; }
    public bool IsActive { get; set; }
    public int? SiteId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CourseDataGuarantor, CourseDataGuarantorSyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate));

        profile.CreateMap<CourseDataGuarantorSyncDto, IntranetMigrator.Domain.Entities.CourseDataGuarantor>();
    }
}
