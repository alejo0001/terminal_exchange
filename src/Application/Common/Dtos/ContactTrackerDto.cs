using System;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class ContactTrackerDto : IMapFrom<ContactTracker>
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public string CountryCode { get; set; }
    public string UserCountryCode { get; set; }
    public string CurrencyCountryCode { get; set; }
    public string Type { get; set; }
    public string AreaUrl { get; set; }
    public int? OriginalCourseId { get; set; }
    public string Title { get; set; }
    public string Enquiry { get; set; }
    public string Url { get; set; }
    public string AccessUrl { get; set; }
    public string TechEnvironment { get; set; }
    public string RequestIp { get; set; }
    public string UserAgent { get; set; }
    public Decimal? Price { get; set; }
    public string Currency { get; set; }
}
