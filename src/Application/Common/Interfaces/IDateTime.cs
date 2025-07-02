using System;

namespace CrmAPI.Application.Common.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }
}