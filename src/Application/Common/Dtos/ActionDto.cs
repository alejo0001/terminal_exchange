using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Enums;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Common.Dtos;

public class ActionDto : IMapFrom<Action>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ContactId { get; set; }
    public int? ProcessId { get; set; }
    public int? OrdersImportedId { get; set; }
    public UserDto User { get; set; }
    public ContactDto Contact { get; set; }
    public ProcessChildDto Process { get; set; }
    public OrdersImportedChildDto OrdersImported { get; set; }
    public DateTime Date { get; set; }
    public DateTime? FinishDate { get; set; }
    public string Type { get; set; }
    public string Outcome { get; set; }
    public EmailActionDto Email { get; set; }
    public IList<AppointmentChildDto> Appointments { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Action, ActionDto>()
            .ForMember(d => d.Contact, opt =>
                opt.MapFrom(dom => dom.Contact))
            .ForMember(d => d.Appointments, opt =>
                opt.MapFrom(dom => dom.Appointments))
            .ForMember(d => d.Process, opt =>
                opt.MapFrom(dom => dom.Process))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => dom.Outcome.ToString().ToLowerInvariant()));
        profile.CreateMap<ActionDto, Action>()
            .ForMember(d => d.Contact, opt =>
                opt.MapFrom(dom => dom.Contact))
            .ForMember(d => d.Appointments, opt =>
                opt.MapFrom(dom => dom.Appointments))
            .ForMember(d => d.Process, opt =>
                opt.MapFrom(dom => dom.Process))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dto => Enum.Parse<ActionType>(dto.Type, true)))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dto => Enum.Parse<ActionOutcome>(dto.Outcome, true)));
    }
}