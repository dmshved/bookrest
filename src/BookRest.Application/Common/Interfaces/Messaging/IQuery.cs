using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces.Messaging;

/// <summary>
/// Represents a query that returns a typed result.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
