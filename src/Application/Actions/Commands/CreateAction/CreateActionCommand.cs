using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CrmAPI.Application.Actions.Commands.CreateAction;

public class CreateActionCommand: ActionCreateDto, IRequest<int>
{
}
    
public class CreateActionCommandHandler : IRequestHandler<CreateActionCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IActionsService _actionService;

    public CreateActionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService,
        IMapper mapper, IActionsService actionService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _actionService = actionService;
    }
        
    public async Task<int> Handle(CreateActionCommand request, CancellationToken cancellationToken)
    {
        var action = _mapper.Map<Action>(request);
            
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Employee.CorporateEmail == _currentUserService.Email, 
                cancellationToken);
        action.UserId = user!.Id;

        action = await _actionService.CreateAction(action, cancellationToken);
        return  action.Id;
    }
}