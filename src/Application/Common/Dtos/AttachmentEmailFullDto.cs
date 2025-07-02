using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AttachmentEmailFullDto : IMapFrom<Attachment>
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string Path { get; set; }
}