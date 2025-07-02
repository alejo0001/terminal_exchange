using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class ActionInfoDto : IMapFrom<Action>
{
    public int Day { get; set; }
    public int AttemptsCalls { get; set; }
    public bool IsActive3X3X3 { get; set; }
    public int MaxAttemptsCalls { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Action, ActionInfoDto>();
    }
}