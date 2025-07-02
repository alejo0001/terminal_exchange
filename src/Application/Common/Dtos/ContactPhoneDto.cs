using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactPhoneDto : IMapFrom<ContactPhone>
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int PhoneTypeId { get; set; }
    public PhoneTypeDto PhoneType { get; set; }
    public string Phone { get; set; }
    public string PhonePrefix { get; set; }
    public bool IsDefault { get; set; }
}