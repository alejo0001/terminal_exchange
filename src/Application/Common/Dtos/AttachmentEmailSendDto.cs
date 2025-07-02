using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AttachmentEmailSendDto : IMapFrom<Attachment>
{
    public string FileName { get; set; }
    public string Base64 { get; set; }
}