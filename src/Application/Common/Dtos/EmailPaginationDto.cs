using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class EmailPaginationDto : IMapFrom<Email>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public UserEmailContactPaginationDto User { get; set; }
    public int? ActionId { get; set; }
    public ActionEmailContactPaginationDto Action { get; set; }
    public string From { get; set; }
    public string FromName { get; set; }
    public string To { get; set; }
    public string Cc { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}