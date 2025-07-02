using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class WhatsappSendDto : IMapFrom<Whatsapp>
{
    public int ContactId { get; set; }

    public int? ActionId { get; set; }

    public int? ProcessId { get; set; }

    public int? WhatsappTemplateId { get; set; }

    public int? CourseId { get; set; }

    public string? From { get; set; }

    public string? FromName { get; set; }

    public string? To { get; set; }

    public string? Message { get; set; }

    public int? ContactLeadId { get; set; }

public void Mapping(Profile profile)
    {
        profile.CreateMap<WhatsappSendDto, Whatsapp>()
            .ForMember(dto => dto.ActionId, op =>
                op.MapFrom(dom => dom.ActionId ?? null))
            .ForMember(dto => dto.ProcessId, op =>
                op.MapFrom(dom => dom.ProcessId ?? null))
            .ForMember(dto => dto.CourseId, op =>
                op.MapFrom(dom => dom.CourseId ?? null))
            .ForMember(dto => dto.From, op =>
                op.MapFrom(dom => dom.From ?? null))
            .ForMember(dto => dto.FromName, op =>
                op.MapFrom(dom => dom.FromName ?? null))
            .ForMember(dto => dto.To, op =>
                op.MapFrom(dom => dom.To ?? null))
            .ForMember(dto => dto.ContactLeadId, op =>
                op.MapFrom(dom => dom.ContactLeadId ?? null));
    }
}
