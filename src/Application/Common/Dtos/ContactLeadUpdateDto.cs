using System;
using System.Collections.Generic;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ContactLeadUpdateDto: IMapFrom<ContactLead>
{
    public int ContactLeadId { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal? EnrollmentPercentage { get; set; }
    public int? Fees { get; set; }
    public string CourseCode { get; set; }
    public DateTime? StartDateCourse { get; set; }
    public DateTime? FinishDateCourse { get; set; }
    public DateTime? ConvocationDate { get; set; }
    public string? CourseTypeBaseCode { get; set; }
    public List<ContactLeadType>? Types { get; set; }
}