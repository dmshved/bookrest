using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces.Messaging;

/// <summary>
/// Represents a query handler that returns a typed result.
/// </summary>
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
