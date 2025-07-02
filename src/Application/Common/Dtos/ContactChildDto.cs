using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactChildDto: IMapFrom<Contact>
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Contact, ContactChildDto>()
            .ForMember(d => d.FirstName, opt =>
                opt.MapFrom(contact => contact.Name))
            .ForMember(d => d.LastName, opt =>
                opt.MapFrom(contact => contact.FirstSurName + " " + contact.SecondSurName))
            .ForMember(d => d.Email, opt =>
                opt.MapFrom(contact => contact.ContactEmail.FirstOrDefault(e => e.IsDefault == true).Email))
            .ForMember(d => d.Phone, opt =>
                opt.MapFrom(contact => contact.ContactPhone.FirstOrDefault(e => e.IsDefault == true).Phone));
    }
}