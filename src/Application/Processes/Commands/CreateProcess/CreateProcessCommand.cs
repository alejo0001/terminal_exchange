using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;

namespace CrmAPI.Application.Processes.Commands.CreateProcess;

public class CreateProcessCommand: ProcessCreateDto, IRequest<int>
{
}
    
public class CreateProcessCommandHandler : IRequestHandler<CreateProcessCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IProcessesService _processesService;

    public CreateProcessCommandHandler(IApplicationDbContext context, IMapper mapper, IProcessesService processesService)
    {
        _context = context;
        _mapper = mapper;
        _processesService = processesService;
    }
        
    public async Task<int> Handle(CreateProcessCommand request, CancellationToken cancellationToken)
    {
        return await _processesService.CreateProcess(request, cancellationToken);
    }
}