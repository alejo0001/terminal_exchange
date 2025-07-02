using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class TraceRecordDto : IMapFrom<TraceRecord>
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public UserDto User { get; set; }
    public int EntityId { get; set; }
    public string EntityType { get; set; }
    public DateTime Date { get; set; }
    public string Property { get; set; }
    public string PreviousValue { get; set; }
    public string NewValue { get; set; }
    public string Notes { get; set; }
    public string Type { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<TraceRecord, TraceRecordDto>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()));
    }
}