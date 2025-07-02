using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessCreateDto : IMapFrom<Process>
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public int ContactId { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string ProcessOrigin { get; set; }
    public string Outcome { get; set; }
    public string Description { get; set; }
    public string? Colour { get; set; }
    public IList<ActionDto> Actions { get; set; }
    public University University { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ProcessCreateDto, Process>()
            .ForMember(d => d.Contact, opt =>
                opt.Ignore())
            .ForMember(d => d.Actions, opt =>
                opt.MapFrom(dom => dom.Actions))
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => Enum.Parse<ProcessType>(dom.Type, true)))
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(dom => Enum.Parse<ProcessStatus>(dom.Status, true)))
            .ForMember(d => d.ProcessOrigin, opt =>
                opt.MapFrom(dom => Enum.Parse<ProcessOrigin>(dom.ProcessOrigin, true)))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => Enum.Parse<ProcessOutcome>(dom.Outcome, true)));
    }
}