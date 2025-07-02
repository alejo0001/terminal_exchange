namespace CrmAPI.Application.Common.Dtos;

public record struct PricesByCodeCourseParamsDto(
    string CurrencyCountryCode,
    string LanguageCode,
    bool Refresh,
    string? CourseCodes = null,
    string? CountryCode = null);