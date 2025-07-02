using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ContactLeadDto : IMapFrom<ContactLead>
{
    public int Id { get; set; }
    public string CountryCode { get; set; }
    public string UserCountryCode { get; set; }
    public string CurrencyCountryCode { get; set; }
    public string AreaUrl { get; set; }
    public string Title { get; set; }
    public int? CourseId { get; set; }
    public CourseDto Course { get; set; }
    public int? CourseDataId { get; set; }
    public CourseDataDto CourseData { get; set; }
    public int? OriginalCourseId { get; set; }
    public string Enquiry { get; set; }
    public int? ContactId { get; set; }
    public ContactDto Contact { get; set; }
    public string Url { get; set; }
    public string AccessUrl { get; set; }
    public Decimal? Price { get; set; }
    public string Currency { get; set; }
    public string CurrencyDisplayFormat { get; set; }
    public string CurrencySymbol { get; set; }
    public DateTime Created { get; set; }
    public bool IsFavourite { get; set; }
    public bool EmailSent { get; set; }
    public bool MessageSent { get; set; }
    public List<SpecialityDto> Specialities { get; set; }
    public FacultyDto Faculty { get; set; }
    
    public string IdContactTypes { get; set; }
    public List<ContactLeadType> Types { get; set; }
    private List<EmailChildViewModel> Emails { get; set; }
    public int CourseCountryId { get; set; }
    public Decimal? FinalPrice { get; set; }
    public Decimal? Discount { get; set; }
    public Decimal? EnrollmentPercentage { get; set; }
    public int? Fees { get; set; }
    public string CourseTypeName { get; set; }
    public DateTime? ContactTrackerDate { get; set; }
    public DateTime? ConvocationDate { get; set; }
    public string CourseTypeBaseCode { get; set; }
    public CourseTypeBaseDto CourseTypeBase { get; set; }
    public DateTime? StartDateCourse { get; set; }
    public DateTime? FinishDateCourse { get; set; }
    public int? LanguageId { get; set; }
    public String? LanguageCode { get; set; }
    public int? CouponOriginId { get; set; }
    public CouponsOriginsDto CouponOrigin { get; set; }
    public University University { get; set; }
    public string CourseCode { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactLead, ContactLeadDto>()
            .ForMember(d => d.LanguageCode,
                opt => opt.MapFrom(f => f.Language != null ? f.Language.Name : null))
            .ForMember(d => d.EmailSent,
                opt => opt.MapFrom(f => f.SentEmail))
            .ForMember(d => d.MessageSent,
                opt => opt.MapFrom(f => f.SentMessage))
            .ForMember(d => d.Types, opt => opt.Ignore()); // << IGNORA este campo
    }

    public static List<ContactLeadType> GetContactLeadTypeList(string? typeString)
    {
        var contactLeadTypes = new List<ContactLeadType>();

        if (string.IsNullOrWhiteSpace(typeString))
            return contactLeadTypes;

        var types = typeString.Split(",", StringSplitOptions.RemoveEmptyEntries);

        foreach (var type in types)
        {
            contactLeadTypes.Add(Enum.TryParse(type, ignoreCase: true, out ContactLeadType value)
                ? value
                : ContactLeadType.Recommended);
        }

        return contactLeadTypes;
    }
}