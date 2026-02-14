namespace SharedKernel.Exceptions
{
    public class NotFoundException : Exception
    {
        protected NotFoundException(Guid id)
            : base($"Record with id {id} not found")
        {
        }
    }
}
