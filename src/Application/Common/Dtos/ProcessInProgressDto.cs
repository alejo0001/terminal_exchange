using System;
using System.Collections.Generic;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessInProgressDto
{
    public int ProcessId { get; set; }
    public int ContactId { get; set; }
    public int? UserId { get; set; }
    public string? Colour { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? InitialDate { get; set; }
    public DateTime? LastActionDate { get; set; }
    public DateTime NextInteractionDate { get; set; }
    public ContactInProgressDto? ContactInProgressDto { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public bool ActiveCall { get; set; }
    public int Attempts { get; set; }
}