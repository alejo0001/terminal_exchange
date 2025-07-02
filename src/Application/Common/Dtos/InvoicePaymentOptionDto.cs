using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class InvoicePaymentOptionDto : IMapFrom<InvoicePaymentOption>
{
    public int Id { get; set; }
    public int InvoicePaymentTypeId { get; set; }
    public string Number { get; set; }
    public string Holder { get; set; }
    public string Status { get; set; }
    public bool HasNonPayment { get; set; }
    public InvoicePaymentTypeDto InvoicePaymentType { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<InvoicePaymentOption, InvoicePaymentOptionDto>()
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(dom => dom.Status.ToString().ToLowerInvariant()));
    }
}