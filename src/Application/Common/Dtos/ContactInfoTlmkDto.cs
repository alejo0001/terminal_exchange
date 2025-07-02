using System;
using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactInfoTlmkDto : IMapFrom<Contact>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FirstSurName { get; set; }
    public string SecondSurName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string IdCard { get; set; }
    public string Profession { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string CountryCode { get; set; }
    public string PostalCode { get; set; }
    public string Department { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Nationality { get; set; }
    public string TitleType {get; set;}
    public string AcademicInstitution { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Contact, ContactInfoTlmkDto>()
            .ForMember(d => d.Email, opt =>
                opt.MapFrom(dom =>
                    dom.ContactEmail.FirstOrDefault(e => e.IsDefault == true).Email))
            .ForMember(d => d.Phone, opt =>
                opt.MapFrom(dom =>
                    dom.ContactPhone.FirstOrDefault(p => p.IsDefault == true).PhonePrefix
                    + dom.ContactPhone.FirstOrDefault(p => p.IsDefault == true).Phone));
    }
}