using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Common.Dtos;

public class ActionChildDto: IMapFrom<Action>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ContactId { get; set; }
    public int? ProcessId { get; set; }
    public int? OrdersImportedId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? FinishDate { get; set; }
    public string Type { get; set; }
    public string Outcome { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Action, ActionChildDto>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => dom.Outcome.ToString().ToLowerInvariant()));
    }
}