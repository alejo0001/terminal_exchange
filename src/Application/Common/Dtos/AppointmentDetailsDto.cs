using AutoMapper;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AppointmentDetailsDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<AppointmentDetailsDto, Appointment>();
    }
}