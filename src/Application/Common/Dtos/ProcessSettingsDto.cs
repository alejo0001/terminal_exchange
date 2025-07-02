using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessSettingsDto: IMapFrom<ProcessSetting>
{
    public int Id { get; set; }
    public int MaxDays { get; set; }
    public int MAxAttempts { get; set; }
    public int StartingFromDay { get; set; }
    public AppointmentType FirstAppointmentType { get; set; }
    public IntranetMigrator.Domain.Enums.ProcessType? ProcessType { get; set; }
    
}