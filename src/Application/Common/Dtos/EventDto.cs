using System;

namespace CrmAPI.Application.Common.Dtos;

public class EventDto
{
    public string UserTag { get; set; }
    public string Subject { get; set; }
    public string EventId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool AllowNewTimeProposals { get; set; }
}