using System.Collections.Generic;

namespace CrmAPI.Application.Templates.Commands.UpdateContentInAllTemplates;

public class AffectedTemplatesViewModel
{
    public string OldContentValue { get; set; }
    public string NewContentValue { get; set; }
    public List<int> AffectedTemplates { get; set; }
    public List<int?> AffectedTemplateLanguages { get; set; }
}