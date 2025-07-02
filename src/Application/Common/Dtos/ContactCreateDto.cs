using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using CrmAPI.Application.Contacts.Commands.CreateContact;
using CroupierAPI.Contracts.Commands;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ContactCreateDto : IMapFrom<Contact>
{
    public string Name { get; set; }
    public string FirstSurName { get; set; }
    public string SecondSurName { get; set; }
    public string? StudentCIF { get; set; }
    public string? FiscalCIF { get; set; }
    public string? Email { get; set; }
    public string? LegalName { get; set; }
    public string? IdCard { get; set; }
    public string? CountryCode { get; set; }
    public string? CurrencyCode { get; set; }
    public int ContactTypeId { get; set; }
    public int ContactStatusId { get; set; }
    public string? MainArea { get; set; }
    public string? MainSpeciality { get; set; }
    public string? Origin { get; set; }
    public string? Profession { get; set; }
    public string? Career { get; set; }
    public int ContactGenderId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? Company { get; set; }
    public DateTime? LastSale { get; set; }
    public string? KeyRegimeCode { get; set; }
    public string? CustomerAccount { get; set; }
    public string? Occupation { get; set; }
    public string? CenterName { get; set; }
    public string? Observations { get; set; }
    public int? OriginContactId { get; set; }
    public DateTime? LastInteraction { get; set; }
    public DateTime? NextInteraction { get; set; }
    public Title? Title { get; set; }
    public string? WorkCenter { get; set; }
    public List<ContactPhoneCreateDto> ContactPhone { get; set; }
    public List<ContactEmailCreateDto> ContactEmail { get; set; }
    public List<ContactAddressCreateDto>? ContactAddress { get; set; }
    public List<ContactLeadCreateDto>? ContactLeads { get; set; }
    public List<ContactTitleCreateDto>? ContactTitles { get; set; }
    public List<ContactLanguageCreateDto>? ContactLanguages { get; set; }
    public List<FacultyContactCreateDto>? Faculties { get; set; }
    public List<SpecialityContactCreateDto>? Specialities { get; set; }
    public bool? CreateProcess { get; set; } = true;
    public ProcessType? Processtype { get; set; } = ProcessType.Records;
    public int? CouponOriginId { get; set; }
    public University University { get; set; }
    public Guid Guid { get; set; }
    public string? RequestIp { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactCreateDto, Contact>()
            .ForMember(d => d.Faculties, opt =>
                opt.MapFrom(dom => new List<Faculty>()))
            .ForMember(d => d.Specialities, opt =>
                opt.MapFrom(dom => new List<Speciality>()));
    }
}