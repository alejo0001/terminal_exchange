using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class InvoicePaymentTypeDto : IMapFrom<InvoicePaymentType>
{
    public int InvoicePaymentTypeId { get; set; }
    public string Name { get; set; }
}