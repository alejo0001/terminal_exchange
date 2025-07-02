using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AppointmentUpdateDto : IMapFrom<Appointment>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ActionId { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<AppointmentUpdateDto, Appointment>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()));
    }
}