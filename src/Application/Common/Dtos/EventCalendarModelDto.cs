using System;
using System.Text.Json.Serialization;

namespace CrmAPI.Application.Common.Dtos;

public class EventCalendarModelDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("subject")]
    public string Subject { get; set; }
    [JsonPropertyName("start")]
    public EventDate Start { get; set; }
    [JsonPropertyName("end")]
    public EventDate End { get; set; }
}

public class EventDate
{
    [JsonPropertyName("datetime")]
    public DateTime DateTime { get; set; }
    [JsonPropertyName("timezone")]
    public string TimeZone { get; set; }
}