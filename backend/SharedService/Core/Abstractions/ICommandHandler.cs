using SharedKernel.Result;

namespace Core.Abstractions
{
    public interface ICommandHandler<TResponse, in TCommand>
        where TCommand : ICommand
    {
        Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task Handle(TCommand command, CancellationToken cancellationToken);
    }
}
