using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Common.Dtos;

public class ActionUpdateDto : IMapFrom<Action>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ContactId { get; set; }
    public int? ProcessId { get; set; }
    public int? OrdersImportedId { get; set; }
    public DateTime Date { get; set; }
    public DateTime FinishDate { get; set; }
    public string ActionType { get; set; }
    public string Outcome { get; set; }
    public IList<AppointmentDto> Appointments { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ActionUpdateDto, Action>()
            .ForMember(d => d.Appointments, opt =>
                opt.MapFrom(dom => dom.Appointments))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.ActionType.ToString().ToLowerInvariant()))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => dom.Outcome.ToString().ToLowerInvariant()));
    }
}