using System;
using System.Text.Json.Serialization;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public readonly record struct WorkScheduleDaysOfWeekDto(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    DayOfWeek DayOfWeek,
    bool Workable,
    TimeSpan? FirstSectionInit,
    TimeSpan? FirstSectionFinish,
    TimeSpan? SecondSectionInit,
    TimeSpan? SecondSectionFinish)
    : IMapFrom<WorkScheduleDayOfWeek>;
