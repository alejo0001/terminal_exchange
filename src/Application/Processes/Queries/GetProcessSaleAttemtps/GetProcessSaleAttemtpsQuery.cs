using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Processes.Queries.GetProcessSaleAttemtps;

public class GetProcessSaleAttemtpsQuery: IRequest<ProcessSaleAttemtpsDto>
{
    public int ProcessId { get; set; }
}

public class GetProcessSaleAttemtpsQueryHandler : IRequestHandler<GetProcessSaleAttemtpsQuery, ProcessSaleAttemtpsDto>
{
        
    private readonly IApplicationDbContext _context;

    public GetProcessSaleAttemtpsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessSaleAttemtpsDto> Handle(GetProcessSaleAttemtpsQuery request, CancellationToken cancellationToken)
    {
        // Buscamos el proceso
        Process process = await _context.Processes
            .Where(p => p.Id == request.ProcessId)
            .FirstOrDefaultAsync(cancellationToken);
            
        // Comprabamos que exista
        if (process == null)
            throw new NotFoundException("Process not found!");

        // Preparamos el DTO que envíaremos al front con el número de intentos de ventas
        ProcessSaleAttemtpsDto dto = new ProcessSaleAttemtpsDto()
        {
            SaleAttemtps = process.SaleAttempts.GetValueOrDefault()
        };

        return dto;
    }
}