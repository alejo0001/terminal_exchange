using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessUpdateDto: IMapFrom<Process>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ContactId { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Outcome { get; set; }
    public string? Description { get; set; }
    public string Colour { get; set; }
    public int? DiscardReasonId { get; set; }
    public string? DiscardReasonObservations { get; set; }
    public University University { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ProcessUpdateDto, Process>()
            .ForMember(d => d.Contact, opt =>
                opt.Ignore())
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => Enum.Parse<ProcessType>(dom.Type, true)))
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(dom => Enum.Parse<ProcessStatus>(dom.Status, true)))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => Enum.Parse<ProcessOutcome>(dom.Outcome, true)))
            .ForMember(d => d.Colour, opt =>
                opt.MapFrom(dom => Enum.Parse<Colour>(dom.Colour, true)));
    }
}