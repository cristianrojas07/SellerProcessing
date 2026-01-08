using Domain.Common;

namespace Application.Abstractions.Messaging;

public interface ICommand { }

public interface ICommandHandler<in TCommand, TResponse> where TCommand : ICommand
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken ct);
}