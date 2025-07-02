using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class TemplateProposalDto : IMapFrom<TemplateProposal>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ProcessType ProcessType { get; set; }
    public int Day { get; set; }    
    public int Attempt { get; set; }
    public Colour Colour { get; set; }
    public bool CourseKnown { set; get; }
    public int? CourseTypeId { get; set; }
    public bool HasToSendEmail { get; set; }
    public bool HasToSendWhatsApp { get; set; }
    public int? TagId { get; set; }
    public string? Team { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<TemplateProposal, TemplateProposalDto>()
            .ForMember(d => d.Team, opt =>
                opt.MapFrom(dom => dom.TagId == null ? "" : dom.Tag.Name));
    }
}