using System.Collections.Generic;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class MessageSendDto
{
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
    public List<int>? ContactLeadList { get; set;  }
    public Colour Colour { get; set; }
}