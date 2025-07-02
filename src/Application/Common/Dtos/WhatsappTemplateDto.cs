using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

[Obsolete("Should be in graceful removal period, because data comes from another entity.")]
public class WhatsappTemplateDto : IMapFrom<WhatsappTemplate>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Body { get; set; }
    public string Type { get; set; }
    public string ProcessType { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<WhatsappTemplate, WhatsappTemplateDto>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()));
    }
}
