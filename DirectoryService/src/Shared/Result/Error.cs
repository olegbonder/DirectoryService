using System.Text.Json.Serialization;

namespace Shared.Result
{
    public record Error
    {
        private Error(string code, string message, ErrorType type)
        {
            Code = code;
            Message = message;
            Type = type;
        }

        public string Code { get; }
        public string Message { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ErrorType Type { get; }

        public static Error None = new(string.Empty, string.Empty, ErrorType.None);
        public static Error Validation(string code, string message)
            => new(code, message, ErrorType.Validation);
        public static Error NotFound(string code, string message)
            => new(code, message, ErrorType.NotFound);
        public static Error Conflict(string code, string message)
            => new(code, message, ErrorType.Conflict);
        public static Error Failure(string code, string message)
            => new(code, message, ErrorType.Failure);

        public Errors ToErrors() => new([this]);
    }

    public enum ErrorType
    {
        /// <summary>
        /// Неизвестная ошибка
        /// </summary>
        None,

        /// <summary>
        /// Ошибка с валидацией
        /// </summary>
        Validation,

        /// <summary>
        /// Ошибка ничего не найдено
        /// </summary>
        NotFound,

        /// <summary>
        /// Ошибка сервера
        /// </summary>
        Failure,

        /// <summary>
        /// Ошибка конфликт
        /// </summary>
        Conflict,

        /// <summary>
        /// Ошибка аутентификации
        /// </summary>
        Authentification,

        /// <summary>
        /// Ошибка авторизации
        /// </summary>
        Authorization,
    }
}
