using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseCountrySyncDto : IMapFrom<Country>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsDeleted { get; set; }
    public int OriginalCountryId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string HreflangCode { get; set; }
    public string LanguageCode { get; set; }
    public string Currency { get; set; }
    public string CurrencyName { get; set; }
    public string CurrencyCode { get; set; }
    public string CurrencyFormat { get; set; }
    public int GeoApiId { get; set; }
    public string Logo { get; set; }
    public string InverseLogo { get; set; }
    public string FlagIcon { get; set; }
    public bool IsActive { get; set; }
    public int? LanguageId { get; set; }
    public string SeoUrl { get; set; }
    public bool? DefaultCountry { get; set; }
    public int? MirrorCountryId { get; set; }
    public bool? GenerateCatalog { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CourseApi.Data.Domain.Country, CourseCountrySyncDto>()
            .ForMember(d => d.Created, opt =>
                opt.MapFrom(dom => dom.CreationDate))
            .ForMember(d => d.LastModified, opt =>
                opt.MapFrom(dom => dom.ModificationDate));
        profile.CreateMap<CourseCountrySyncDto, CourseCountry>();
    }
}
