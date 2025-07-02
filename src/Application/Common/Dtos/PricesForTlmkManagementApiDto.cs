using CrmAPI.Contracts.Dtos;

namespace CrmAPI.Application.Common.Dtos;

public record struct PricesForTlmkManagementApiDto(string CourseCode, int CountryId, string CourseTypeBaseCode, string ProcessType) : IPricesForTlmkFromManagementApiDto;