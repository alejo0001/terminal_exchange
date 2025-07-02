using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class EmployeeDto : IMapFrom<Employee>
{
    public EmployeeGender Gender { get; set; }
    public string CorporatePhonePrefix { get; set; }    
    public string CorporatePhone { get; set; }    
    public string CorporateEmail { get; set; }
}