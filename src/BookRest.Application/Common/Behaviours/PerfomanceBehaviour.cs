using BookRest.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BookRest.Application.Common.Behaviours;

public partial class PerfomanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new Stopwatch();
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public PerfomanceBehaviour(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next(cancellationToken);

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var applicationUserId = _currentUserService.ApplicationUserId;
            var userName = string.Empty;

            if (applicationUserId.HasValue)
            {
                userName = await _identityService.GetUserNameAsync(applicationUserId.Value);
            }

            LogRequest(requestName, elapsedMilliseconds, applicationUserId, userName, request);
        }

        return response;
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "BookRest Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@ApplicationUserId} {@UserName} {@Request}")]
    private partial void LogRequest(
        string name,
        long elapsedMilliseconds,
        Guid? applicationUserId,
        string? userName,
        TRequest request);
}
