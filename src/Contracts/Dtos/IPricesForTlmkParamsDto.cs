namespace CrmAPI.Contracts.Dtos;

public interface IPricesForTlmkParamsDto
{
    string AreaUrl { get; }

    string CurrencyCode { get; }

    int CountryId { get; }

    string CourseCode { get; }

    string LanguageCode { get; }

    string CourseTypeBaseCode { get; }

    string ProcessType { get; }
}
