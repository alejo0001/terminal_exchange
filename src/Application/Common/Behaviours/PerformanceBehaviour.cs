﻿using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;

    public PerformanceBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds <= 500) return response;
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;

        _logger.LogWarning(
            "CrmAPI Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserEmail} {@Request}",
            requestName, elapsedMilliseconds, userId, userEmail, request);

        return response;
    }
}