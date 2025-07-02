using System.Collections.Generic;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class TemplateProposalTemplateDto: IMapFrom<TemplateProposalTemplate>
{
    public int TemplateProposalId { get; set; }

    public int TemplateId { get; set; }

    public TemplateDetailsDto Template { get; set; }
}