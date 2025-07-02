using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CroupierAPI.Contracts.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProcessType = IntranetMigrator.Domain.Enums.ProcessType;

namespace CrmAPI.Application.ProcessSettingsTypes.Queries.GetProcessSettingsTypes
{
    [Authorize]
    public class GetProcessSettingsTypeQuery: IRequest<List<ProcessTypeDto>>
    {
        public bool IsCouponsOnly { get; set; } = false;
    }
    
    public class GetProcessSettingsTypeQueryHandler: IRequestHandler<GetProcessSettingsTypeQuery, List<ProcessTypeDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetProcessSettingsTypeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProcessTypeDto>> Handle(GetProcessSettingsTypeQuery request, CancellationToken ct)
        {
            var processType = await _context.ProcessSettings
                .Where(ps => !ps.IsDeleted && ps.ProcessType != null)
                .Select(pt => pt.ProcessType)
                .ToListAsync(ct);

            if (request.IsCouponsOnly)
            {
                processType = processType
                    .Where(pt => pt.HasValue 
                                 && pt.Value
                                     .ToString()
                                     .IndexOf("Coupons", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }
            
            var list = processType
                .Select(pt => new ProcessTypeDto
                {
                    Label = Enum.GetName(pt.Value)?.ToLower().Replace("_","") ?? string.Empty,
                    Value = Enum.GetName(pt.Value)?.Replace("_","") ?? string.Empty,
                })
                .ToList();

            return list;
        }
    }
}