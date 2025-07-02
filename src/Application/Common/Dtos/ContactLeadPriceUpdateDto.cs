using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactLeadPriceUpdateDto: IMapFrom<ContactLead>
{
    public int ContactLeadId { get; set; }
    public decimal? Price { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal? EnrollmentPercentage { get; set; }
    public int? Fees { get; set; }
}