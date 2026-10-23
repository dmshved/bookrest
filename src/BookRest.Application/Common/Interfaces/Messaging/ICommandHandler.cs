using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces.Messaging;

/// <summary>
/// Represents a command handler that returns a typed result.
/// </summary>
public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}

/// <summary>
/// Represents a command handler that returns result.
/// </summary>
public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

