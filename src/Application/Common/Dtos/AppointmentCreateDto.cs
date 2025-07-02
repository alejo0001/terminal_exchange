using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AppointmentCreateDto : IMapFrom<Appointment>
{
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<AppointmentCreateDto, Appointment>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()));
    }
}