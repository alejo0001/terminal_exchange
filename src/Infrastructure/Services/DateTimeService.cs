using System;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now =>  DateTime.UtcNow;
}