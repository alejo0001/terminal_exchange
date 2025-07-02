using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ActionCreateDto : IMapFrom<IntranetMigrator.Domain.Entities.Action>
{
    public int? UserId { get; set; }
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
    public DateTime Date { get; set; }
    public string ActionType { get; set; }
    public string Outcome { get; set; }
    public IList<AppointmentDto> Appointments { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ActionCreateDto, IntranetMigrator.Domain.Entities.Action>()
            .ForMember(d => d.Appointments, opt =>
                opt.MapFrom(dom => dom.Appointments))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dto => Enum.Parse<ActionType>(dto.ActionType, true)))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dto => Enum.Parse<ActionOutcome>(dto.Outcome, true)));
    }
}