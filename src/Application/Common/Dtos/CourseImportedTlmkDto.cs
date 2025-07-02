using System;
using System.Collections.Generic;

namespace CrmAPI.Application.Common.Dtos;

public readonly record struct CourseImportedTlmkDto(
    string CourseCode,
    int CourseId,
    int OriginalCourseId,
    string Title,
    string BaseTitle,
    int Price,
    float TotalAmount,
    bool SplitPaymentOnly,
    decimal ScholarshipPercent,
    decimal ScholarshipPrice,
    decimal ScholarshipEnrolmentAmount,
    int FeesCount,
    decimal Interest,
    float InitialFee,
    float RecurrentFee,
    float TotalPromoAmount,
    float InitialPromoFee,
    float RecurrentPromoFee,
    DateTime? StartDateTime,
    string StartDate,
    string FinishDate,
    bool Intensive,
    string CourseType,
    string CourseTypeName,
    int GuarantorId,
    string GuarantorName,
    string GuarantorLogoUrl,
    string Url,
    float Hours,
    float Credits,
    string CreditAcronym,
    string Duration,
    int DurationCourseInDays,
    int? Months,
    int? Weeks,
    int? Days,
    string Dossier,
    string PackType,
    CourseImportedTlmkDto.CountryDto Country,
    List<CourseImportedTlmkDto.AreaDto> Areas
    //List<SpecialityFlattenDto> Specialities
)
{
    public readonly record struct CountryDto(
        string Name,
        string Code,
        string Currency,
        string CurrencyCode);

    public readonly record struct AreaDto(
        string Name,
        string Code,
        string Label,
        string Color,
        string Url,
        string BaseUrl);
}
