using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessFullDto: IMapFrom<Process>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ContactId { get; set; }
    public int OrdersImportedId { get; set; }
    public DateTime Created { get; set; }
    public UserDto User { get; set; }
    public ContactDto Contact { get; set; }
    public OrdersImportedChildDto OrdersImported { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Outcome { get; set; }
    public string Description { get; set; }
    public string Colour { get; set; }
    public List<ActionChildDto> Actions { get; set; }
    public DateTime? InitialDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Process, ProcessFullDto>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()))
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(dom => dom.Status.ToString().ToLowerInvariant()))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => dom.Outcome.ToString().ToLowerInvariant()))
            .ForMember(d => d.Colour, opt =>
                opt.MapFrom(dom => dom.Colour.ToString().ToLowerInvariant()));
    }
}