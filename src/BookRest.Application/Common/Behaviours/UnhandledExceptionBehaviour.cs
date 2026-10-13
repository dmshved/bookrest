using Microsoft.Extensions.Logging;

namespace BookRest.Application.Common.Behaviours;

public partial class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception)
        {
            var requestName = typeof(TRequest).Name;

            LogException(exception, requestName, request);

            throw;
        }
    }

    [LoggerMessage(
        LogLevel.Error,
        Message = "BookRest Request: Unhandled Exception for Request {Name} {@Request}")]
    private partial void LogException(
        Exception exception,
        string name,
        TRequest request);
}
