using System.Collections.Generic;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class EmailFullDto : IMapFrom<Email>
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public ContactEmailFullDto Contact { get; set; }
    public int? UserId { get; set; }
    public UserEmailFullDto User { get; set; }
    public int? ActionId { get; set; }
    public ActionEmailFullDto Action { get; set; }
    public string From { get; set; }
    public string FromName { get; set; }
    public string To { get; set; }
    public string Cc { get; set; }
    public string Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string Message { get; set; }
    public int? EmailTemplateId { get; set; }
    public List<AttachmentEmailFullDto> Attachments { get; set; }
}
