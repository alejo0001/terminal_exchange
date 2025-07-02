using System;
using System.Collections.Generic;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class InvoicePaymentOptionUpdateDto : IMapFrom<InvoicePaymentOption>
{
    public int Id { get; set; }
    public int InvoicePaymentTypeId { get; set; }
    public string Number { get; set; }
    public string Holder { get; set; }
    public string Status { get; set; }
    public IList<ContactChildDto> Contacts { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<InvoicePaymentOptionUpdateDto, InvoicePaymentOption>()
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(dto => Enum.Parse<InvoicePaymentOptionStatus>(dto.Status, true)));
    }
}