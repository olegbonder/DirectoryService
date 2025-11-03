namespace Shared.Result
{
    public class Result
    {
        protected Result()
        {
            IsSuccess = true;
            Error = Error.None;
        }

        protected Result(Error error)
        {
            IsSuccess = false;
            Error = error;
        }

        public Error Error { get; set; }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public static Result Success() => new();

        public static Result Failure(Error error) => new(error);

        public static implicit operator Result(Error error) => Failure(error);
    }
}