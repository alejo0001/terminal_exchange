using System;
using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactDto : IMapFrom<Contact>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FirstSurName { get; set; }
    public string SecondSurName { get; set; }
    public string Phone { get; set; }
    public string StudentCIF { get; set; }
    public string FiscalCIF { get; set; }
    public string Email { get; set; }
    public string? LegalName { get; set; }
    public string IdCard { get; set; }
    public string CountryCode { get; set; }
    public int ContactTypeId { get; set; }
    public ContactTypeDto Type { get; set; }
    public int ContactStatusId { get; set; }
    public ContactStatusDto Status { get; set; }
    public string MainArea { get; set; }
    public string MainSpeciality { get; set; }
    public string Origin { get; set; }
    public string Profession { get; set; }
    public string Career { get; set; }
    public int ContactGenderId { get; set; }
    public ContactGenderDto Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Nationality { get; set; }
    public string Company { get; set; }
    public DateTime? LastSale { get; set; }
    public string? KeyRegimeCode { get; set; }
    public string? CustomerAccount { get; set; }
    public string? Occupation { get; set; }
    public string? CenterName { get; set; }
    public string? WorkCenter { get; set; }
    public string? Observations { get; set; }
    public int? OriginContactId { get; set; }
    public DateTime? LastInteraction { get; set; }
    public DateTime? NextInteraction { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Contact, ContactDto>()
            .ForMember(c => c.NextInteraction, opt =>
                opt.MapFrom(dom =>
                    dom.NextInteraction == null ? DateTime.Now : dom.NextInteraction))
            .ForMember(c => c.Phone, opt =>
                opt.MapFrom(dom =>
                    (dom.ContactPhone.FirstOrDefault(p => !p.IsDeleted)!.PhonePrefix
                     + " " + dom.ContactPhone.FirstOrDefault(p => !p.IsDeleted)!.Phone).Trim()
                ))
            .ForMember(c => c.Email, opt =>
                opt.MapFrom(dom => 
                    dom.ContactEmail.FirstOrDefault(p => p.IsDefault == true && !p.IsDeleted)!.Email));

    }
}