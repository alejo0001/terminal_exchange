using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ContactUpdateDto : IMapFrom<Contact>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? FirstSurName { get; set; }
    public string? SecondSurName { get; set; }
    public string? StudentCIF { get; set; }
    public string? FiscalCIF { get; set; }
    public string? LegalName { get; set; }
    public string? IdCard { get; set; }
    public string? CountryCode { get; set; }
    public string? CurrencyCode { get; set; }
    public int? ContactTypeId { get; set; }
    public int? ContactStatusId { get; set; }
    public string? Profession { get; set; }

    public int? ContactGenderId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }

    public Title? Title { get; set; }
    public string? WorkCenter { get; set; }
    public bool? DontWantCalls { get; set; }
    public Guid? Guid { get; set; }
    public List<ContactPhoneUpdateDto>? ContactPhone { get; set; }
    public List<ContactEmailUpdateDto>? ContactEmail { get; set; }
    public List<ContactAddressUpdateDto>? ContactAddress { get; set; }
    public List<ContactTitleUpdateDto>? ContactTitles { get; set; }
    public List<ContactLanguageUpdateDto>? ContactLanguages { get; set; }
    public List<FacultyDto>? Faculties { get; set; }
    public List<SpecialityDto>? Specialities { get; set; }    
    public int? CouponOriginId { get; set; }
    public string? RequestIp { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactUpdateDto, Contact>();
    }
}
