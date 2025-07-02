namespace CrmAPI.Contracts.Dtos;

public interface IPricesForTlmkFromManagementApiDto
{
    string CourseCode { get; }

    int CountryId { get; }

    string CourseTypeBaseCode { get; }

    string ProcessType { get; }
}