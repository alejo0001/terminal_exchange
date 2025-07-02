using System.Collections.Generic;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class EmailSendDto : IMapFrom<Email>
{
    public int ContactId { get; set; }
    public List<int>? ContactLeadId { get; set;  }
    public int? CourseId { get; set; }
    public int ProcessId { get; set; }
    public string To { get; set; }
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<string>? Dossiers { get; set; }
    public Colour Colour { get; set;  }
    public string? EmailDefault { get; set;  }
}