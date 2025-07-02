using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ContactFullDto : IMapFrom<Contact>
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string FirstSurName { get; set; }
    public string SecondSurName { get; set; }
    public string StudentCIF { get; set; }
    public string FiscalCIF { get; set; }
    public string Email { get; set; }
    public string? LegalName { get; set; }
    public string IdCard { get; set; }
    public string? CountryCode { get; set; }
    public CourseCountryDto? Country { get; set; }
    public int? CurrencyId { get; set; }
    public CurrencyDto? Currency { get; set; }
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
    public string? Observations { get; set; }
    public int? OriginContactId { get; set; }
    public Title? Title { get; set; }
    public string? WorkCenter { get; set; }
    public ActiveCallDto ActiveCall { get; set; }
    public DateTime? LastInteraction { get; set; }
    public DateTime? NextInteraction { get; set; }
    public bool? DontWantCalls { get; set; }
    public List<FacultyDto> Faculties { get; set; }
    public List<SpecialityDto> Specialities { get; set; }
    public List<ProcessChildViewModel> Processes { get; set; }
    public List<ContactPhoneDto> ContactPhone { get; set; }
    public List<ContactEmailDto> ContactEmail { get; set; }
    public List<ContactAddressDto> ContactAddress { get; set; } 
    public List<ContactTitleDto> ContactTitles { get; set; }
    public List<ContactLanguageDto> ContactLanguages { get; set; }
    public int? SaleAttempts { get; set; }
    public bool IsClient { get; set; }
    public string? RequestIp { get; set; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Contact, ContactFullDto>()
            .ForMember(dto => dto.Email,
                expression => expression.MapFrom(
                    contact => contact.ContactEmail.FirstOrDefault(e => e.IsDefault == true).Email)
            );
    }
}