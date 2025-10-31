using Shared.Result;

namespace DirectoryService.Presenters.EndpointResult
{
    public record Envelope
    {
        public object? Result { get; }
        public Errors? ErrorList { get; }
        public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());
        public DateTime TimeGenerated { get; }

        private Envelope(object? result, Errors? errorList)
        {
            Result = result;
            ErrorList = errorList;
            TimeGenerated = DateTime.Now;
        }

        public static Envelope Ok(object? result) =>
            new(result, null);

        public static Envelope Error(Errors errors) =>
            new(null, errors);
    }

    public record Envelope<T>
    {
        public T? Result { get; }
        public Errors? ErrorList { get; }
        public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());
        public DateTime TimeGenerated { get; }

        private Envelope(T? result, Errors? errorList)
        {
            Result = result;
            ErrorList = errorList;
            TimeGenerated = DateTime.Now;
        }

        public static Envelope<T> Ok(T? result) =>
            new(result, null);

        public static Envelope<T> Error(Errors errors) =>
            new(default, errors);
    }
}
