using CrmAPI.Application.Common.Dtos;

namespace CrmAPI.Application.Templates.Queries.GetTemplateBundleProposal;

public class TemplateBundleProposalViewModel
{
    public TemplateDto EmailTemplate { get; set; }
    public TemplateDto WhatsAppTemplate { get; set; }
    public bool HasToSendEmail { get; set; }
    public bool HasToSendWhatsapp { get; set; }
}