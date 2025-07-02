using System.Collections.Generic;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactFacultiesCreateDto : IMapFrom<ContactFaculty>
{
    public List<int> FacultiesId { get; set; }
    public int ContactId { get; set; }
        
}