using System.Collections.Generic;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactSpecialitiesCreateDto : IMapFrom<ContactSpeciality>
{
    public List<int> SpecialitiesId { get; set; }
    public int ContactId { get; set; }
        
}