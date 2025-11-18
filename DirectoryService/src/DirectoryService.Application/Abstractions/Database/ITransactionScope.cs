using Shared.Result;

namespace DirectoryService.Application.Abstractions.Database
{
    public interface ITransactionScope : IDisposable
    {
        Result Commit();

        Result RollBack();
    }
}
