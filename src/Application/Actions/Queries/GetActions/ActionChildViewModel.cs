using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Mappings;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Actions.Queries.GetActions;


// TODO: Refactorizar estae DTO zurdo
public class ActionChildViewModel : IMapFrom<Action>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ContactId { get; set; }
    public int? ProcessId { get; set; }
    public int? OrdersImportedId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? FinishDate { get; set; }
    public string ActionType { get; set; }
    public string Outcome { get; set; }
    public ReassignmentDto? Reassignment { get; set; }
    public UserDto User { get; set; }
    public IList<AppointmentChildDto> Appointments { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Action, ActionChildViewModel>()
            .ForMember(d => d.ActionType, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => dom.Outcome.ToString().ToLowerInvariant()))
            .ForMember(d => d.Appointments, opt =>
                opt.MapFrom(dom => dom.Appointments));
    }
}