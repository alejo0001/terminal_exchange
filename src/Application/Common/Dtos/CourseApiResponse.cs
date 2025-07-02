using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Application.Common.Dtos;

/// <summary>
///     To compensate CourseApi response structure.
/// </summary>
/// <param name="Data">Wrapped data, what we actually need.</param>
/// <remarks>
///     For example, see  <see cref=" ICourseUnApiClient.GetPricesForTlmk" />.
/// </remarks>
public readonly record struct CourseApiResponse<TModel>(TModel Data);
