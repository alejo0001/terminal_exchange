using System.Collections.Generic;

namespace CrmAPI.Infrastructure.Emails.Models;

public class MailDto
{
    public string from { get; set; }
    public string fromName { get; set; }
    public List<string> to { get; set; }
    public List<string> cc { get; set; }
    public List<string> bcc { get; set; }
    public string subject { get; set; }
    public string bodyHtml { get; set; }
    public List<AttachElment> attachList { get; set; }
}