using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CountryChildViewModel : IMapFrom<Country>
{
    public string Name { get; set; }
    public string CountryCode { get; set; }
}