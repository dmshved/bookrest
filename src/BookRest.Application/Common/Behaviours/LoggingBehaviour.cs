using BookRest.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace BookRest.Application.Common.Behaviours;

public partial class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>

{
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        Guid? applicationUserId = _currentUserService.ApplicationUserId;
        string? userName = string.Empty;

        if (applicationUserId.HasValue)
        {
            userName = await _identityService
                .GetUserNameAsync(applicationUserId.Value);
        }

        // Fix CA1873: Avoid potentially expensive logging
        // Learn more: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1873
        if (_logger.IsEnabled(LogLevel.Information))
        {
            LogRequest(requestName, applicationUserId, userName, request);
        }
    }

    // Fix CA1848: Use the LoggerMessage delegates
    // Learn more: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1848
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "BookRest Request: {Name} {@ApplicationUserId} {@UserName} {@Request}")]
    private partial void LogRequest(
        string name,
        Guid? applicationUserId,
        string? userName,
        TRequest request);
}
