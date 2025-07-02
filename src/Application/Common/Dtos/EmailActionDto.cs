using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class EmailActionDto : IMapFrom<Email>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public UserDto User { get; set; }
    public string From { get; set; }
    public string FromName { get; set; }
    public string To { get; set; }
    public string Cc { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}