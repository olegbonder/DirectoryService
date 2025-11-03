namespace Shared.Result
{
    public sealed class Result<TValue> : Result
    {
        private Result(TValue value) => _value = value;

        private Result(Error error)
            : base(error)
        {
        }

        private readonly TValue _value;

        public TValue Value => IsSuccess
            ? _value
            : throw new ApplicationException("Result is not success");

        public static Result<TValue> Success(TValue value) => new(value);

        public static new Result<TValue> Failure(Error error) => new(error);

        public static implicit operator Result<TValue>(TValue value) => new(value);

        public static implicit operator Result<TValue>(Error error) => new(error);

        public static implicit operator TValue(Result<TValue> value) => value._value;
    }
}
