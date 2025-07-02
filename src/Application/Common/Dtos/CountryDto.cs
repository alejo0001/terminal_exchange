using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CountryDto : IMapFrom<Country>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
    public int CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; }
    public string PhonePrefix { get; set; }
    public string DateFormat { get; set; }
}