using CrmAPI.Application.Common.Mappings;
using Contact = CourseApi.Data.Domain.Contact;

namespace CrmAPI.Application.Common.Dtos;

public class ContactInfoDto : IMapFrom<Contact>
{
    public int Id { get; set; }
    public int ProcessId { get; set; }
    public bool CanCreateProcess { get; set; }
    public bool IsBlackList { get; set; }
    public bool? Replaceable { get; set; }
}