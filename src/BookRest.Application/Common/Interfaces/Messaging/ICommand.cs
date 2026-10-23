using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces.Messaging;

/// <summary>
/// Represents a command that returns a typed result.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Represents a command that returns a result.
/// </summary>
public interface ICommand : IRequest<Result>
{
}
