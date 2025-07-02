using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class TemplateProposalCreateDto : IMapFrom<TemplateProposal>
{
    public string? Name { get; set; }

    public ProcessType ProcessType { set; get; }

    public int Day { get; set; }

    public int Attempt { get; set; }

    public Colour Colour { get; set; }

    public bool CourseKnown { set; get; }

    public int? CourseTypeId { get; set; }

    public bool HasToSendEmail { get; set; }

    public bool HasToSendWhatsApp { get; set; }

    public int? TagId { get; set; }

   

 
}