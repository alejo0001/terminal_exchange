using System;
using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class GuarantorSyncDto : IMapFrom<Guarantor>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public int OriginalGuarantorId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string LongName { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
    public string Logo { get; set; }
    public string Url { get; set; }
    public string SeoUrl { get; set; }
    public string InverseLogo { get; set; }
    public string Code { get; set; }
    public bool MainGuarantor { get; set; }
    public int? Order { get; set; }
    public bool IsActive { get; set; }
    public string Gender { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Guarantor, GuarantorSyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate));
        profile.CreateMap<GuarantorSyncDto, IntranetMigrator.Domain.Entities.Guarantor>();
    }
}
