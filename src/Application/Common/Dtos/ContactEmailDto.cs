using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactEmailDto : IMapFrom<ContactEmail>
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int EmailTypeId { get; set; }
    public EmailTypeDto EmailType { get; set; }
    public string Email { get; set; }
    public bool IsDefault { get; set; }
}