using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class LeadDto : IMapFrom<Contact>
{
    public int ContactId { get; set; }
    public Guid ContactGuid { get; set; }
    public string? Name { get; set; }
    public string? FirstSurname { get; set; }
    public string? SecondSurname { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Nif { get; set; }
    public DateTime Created { get; set; }
    public int? ContactGenderId { get; set; }
    public int? ContactStatusId { get; set; }
    public string? CountryCode { get; set; }
    public string? NationalityCode { get;set; }
    public List<int>? LanguagesId { get; set; }
    public List<int>? FacultiesId { get; set; }
    public List<int>? SpecialitiesId { get; set; }
    public List<int?>? CoursesId { get; set; }
    public List<string>? Emails { get; set; }
    public List<string>? Phones { get; set; }
    public string? Provenance { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Contact, LeadDto>();
    }
}