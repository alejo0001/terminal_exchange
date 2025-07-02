using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class EmailTemplateBodyDto : IMapFrom<EmailTemplate>
{
    public string Body { get; set; }
}