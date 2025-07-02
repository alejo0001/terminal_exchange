using CrmAPI.Contracts.Dtos;

namespace CrmAPI.Application.Common.Dtos;

public record struct PricesForTlmkParamsDto(
    string AreaUrl,
    string CurrencyCode,
    int CountryId,
    string CourseCode,
    string LanguageCode,
    string CourseTypeBaseCode,
    string ProcessType)
    : IPricesForTlmkParamsDto;
