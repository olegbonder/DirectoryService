using SharedKernel.Result;

namespace Core.Database
{
    public interface ITransactionScope : IDisposable
    {
        Result Commit();

        Result RollBack();
    }
}
