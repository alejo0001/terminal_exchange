using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class UserEmailContactPaginationDto : IMapFrom<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
}