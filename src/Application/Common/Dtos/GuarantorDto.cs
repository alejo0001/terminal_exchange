using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class GuarantorDto : IMapFrom<Guarantor>
{
    public int OriginalGuarantorId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string LongName { get; set; }
    public string Label { get; set; }
}