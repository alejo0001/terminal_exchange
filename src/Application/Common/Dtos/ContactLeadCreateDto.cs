using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ContactLeadCreateDto : IMapFrom<ContactLead>
{
    public int? CourseId { get; set; }
    public int? CourseDataId { get; set; }
    public int ContactId { get; set; }
    public string? Url { get; set; }        
    public string? CountryCode { get; set; }
    public int CourseCountryId { get; set; }
    public decimal? Price { get; set; }
    public decimal? FinalPrice { get; set; }
    public string? Currency { get; set; }
    public string CourseTypeName { get; set; }
    public string FacultyName { get; set; }
    public string? AreaUrl { get; set; }
    public int? FacultyId { get; set; }
    public decimal? Discount { get; set; }
    public decimal? EnrollmentPercentage { get; set; }
    public int? Fees { get; set; }
    public bool IsFavourite { get; set; }
    public bool EmailSent { get; set; }
    public bool MessageSent { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? ConvocationDate { get; set; }
    public List<ContactLeadType> Types { get; set; }
    public int ProcessId { get; set; }
    public string CourseTypeBaseCode { get; set; }
    public DateTime? StartDateCourse { get; set; }
    public DateTime? FinishDateCourse { get; set; }
    public String LanguageCode { get; set; }
    public int? LanguageId { get; set; }
    public University University { get; set; }
    public string? Title { get; set; }
    public string CourseCode { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactLeadCreateDto, ContactLead>()
            .ForMember(d => d.Types, opt =>
                opt.MapFrom(dom => "1"))
            .ForMember(d => d.SentEmail, 
                opt =>
                    opt.MapFrom(f => f.EmailSent));
    }
}