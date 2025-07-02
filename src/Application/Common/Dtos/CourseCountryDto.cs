using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CourseCountryDto : IMapFrom<CourseCountry>
{
    public int Id { get; set; }

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

    public LanguageDto Language { get; set; }

    public int? LanguageId { get; set; }

    public string SeoUrl { get; set; }

    public bool? DefaultCountry { get; set; }

    public int? MirrorCountryId { get; set; }

    public bool? GenerateCatalog { get; set; }
}