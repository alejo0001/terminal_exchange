using System;
using FluentValidation;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Templates.Queries.GetTemplates;

public class GetTemplateQueryValidator : AbstractValidator<GetTemplatesQuery>
{
    public GetTemplateQueryValidator()
    {
        RuleFor(v => v.ProcessType)
            .Must(processType => Enum.TryParse(typeof(ProcessType), processType, true, out _)).WithMessage("Invalid process type");
            
            
        RuleFor(v => v.TemplateType)
            .Must(templateType => Enum.TryParse(typeof(TemplateType), templateType, true, out _)).WithMessage("Invalid template type");
    }
}