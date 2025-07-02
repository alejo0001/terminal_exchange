using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactTypeDto : IMapFrom<ContactType>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }
}