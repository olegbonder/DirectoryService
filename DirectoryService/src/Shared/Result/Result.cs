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

    public sealed class Result<TValue> : Result
    {
        public Result(TValue vakue) => _value = vakue;

        public Result(Error error)
            : base(error)
        {
        }

        private readonly TValue _value;

        public TValue Value => IsSuccess
            ? _value
            : throw new ApplicationException("Result is not success");

        public static Result<TValue> Success(TValue value) => new(value);

        public static Result<TValue> Failure(Error error) => new(error);

        public static implicit operator Result<TValue>(TValue value) => new(value);

        public static implicit operator Result<TValue>(Error error) => new(error);

        public static implicit operator TValue(Result<TValue> value) => value._value;
    }
}
