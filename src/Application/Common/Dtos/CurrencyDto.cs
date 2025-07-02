using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class CurrencyDto : IMapFrom<Currency>
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; }
    public string CurrencySymbol { get; set; }
    public string Name { get; set; }
    public string? CurrencyDisplayFormat { get; set; }
}