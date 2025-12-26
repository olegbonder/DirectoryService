namespace Shared.Result
{
    public class Result
    {
        protected Result()
        {
            IsSuccess = true;
            Errors = Error.None;
        }

        protected Result(Error error)
        {
            IsSuccess = false;
            Errors = error;
        }

        protected Result(Errors errors)
        {
            IsSuccess = false;
            Errors = errors;
        }

        public Errors Errors { get; set; }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public static Result Success() => new();

        public static Result Failure(Error error) => new(error);

        public static Result Failure(Errors errors) => new(errors);

        public static implicit operator Result(Error error) => Failure(error);

        public static implicit operator Result(Errors errors) => Failure(errors);
    }
}