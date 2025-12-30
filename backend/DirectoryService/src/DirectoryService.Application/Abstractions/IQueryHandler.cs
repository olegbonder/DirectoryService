using Shared.Result;

namespace DirectoryService.Application.Abstractions
{
    public interface IQueryHandler<TResponse, in TQuery>
        where TQuery : class
    {
        Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
    }
}
