// Pipeline behaviour that enforces validation for a request. 
// - Checks for Validators on the request type
// - Validates request's validators
// - Throws ValidationException if validation failed 

using ValidationException = BookRest.Application.Common.Exceptions.ValidationException;

namespace BookRest.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators): IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Count() != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}