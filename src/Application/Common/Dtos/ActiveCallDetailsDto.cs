using System;
using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using Action = IntranetMigrator.Domain.Entities.Action;

namespace CrmAPI.Application.Common.Dtos;

public class ActiveCallDetailsDto : IMapFrom<Action>
{
    public int? Id { get; set; }
    public int? ContactId { get; set; }
    public int? ProcessId { get; set; }
    public string Name { get; set; }
    public string FirstSurName { get; set; }
    public string SecondSurName { get; set; }
    public string ProcessColour { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public String Outcome { get; set; }
    public DateTime? Date { get; set; }
        
    public ProcessDto Process { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Action, ActiveCallDetailsDto>()
            .ForMember(dto => dto.Name, expression =>
                expression.MapFrom(action => action.Contact.Name))
            .ForMember(dto => dto.FirstSurName, expression =>
                expression.MapFrom(action => action.Contact.FirstSurName))
            .ForMember(dto => dto.SecondSurName, expression =>
                expression.MapFrom(action => action.Contact.SecondSurName))
            .ForMember(dto => dto.ContactEmail, expression => 
                expression.MapFrom(action => action.Contact.ContactEmail.FirstOrDefault().Email))
            .ForMember(dto => dto.ContactPhone, expression => 
                expression.MapFrom(action => action.Contact.ContactPhone.FirstOrDefault().Phone));
    }
}