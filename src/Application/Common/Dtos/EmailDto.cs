using System.Collections.Generic;

namespace CrmAPI.Application.Common.Dtos;

public class EmailDto
{
    public string From { get; set; }
    public string Subject { get; set; }
    public List<string> Receivers { get; set; }
    public List<string> CcReceivers { get; set; }
    public List<string> BccReceivers { get; set; }
    public string BodyHtml { get; set; }
    public List<AttachmentDto> Attachments { get; set; }
}