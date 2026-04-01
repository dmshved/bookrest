// Pipeline behaviour that logs any unhandled exception thrown during the request pipeline.
// - Invokes the next pipeline delegate
// - Catches any exception thrown downstream
// - Logs error with request exception and user information
// - Rethrows the exceptions so the upper layers taking care of it

using Microsoft.Extensions.Logging;

namespace BookRest.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "BookRest Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }
}