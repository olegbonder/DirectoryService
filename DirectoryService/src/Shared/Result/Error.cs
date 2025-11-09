using System.Text.Json.Serialization;

namespace Shared.Result
{
    public record Error
    {

        private const string SEPARATOR = "||";
        private Error(string code, string message, ErrorType type, string? invalidField = null)
        {
            Code = code;
            Message = message;
            Type = type;
            InvalidField = invalidField;
        }

        public string Code { get; }
        public string Message { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ErrorType Type { get; }
        public string? InvalidField { get; }

        public static Error None = new(string.Empty, string.Empty, ErrorType.None);
        public static Error Validation(string code, string message, string? invalidField = null)
            => new(code, message, ErrorType.Validation, invalidField);
        public static Error NotFound(string code, string message)
            => new(code, message, ErrorType.NotFound);
        public static Error Conflict(string code, string message)
            => new(code, message, ErrorType.Conflict);
        public static Error Failure(string code, string message)
            => new(code, message, ErrorType.Failure);

        public Errors ToErrors() => new([this]);
        public string Serialize() => string.Join(SEPARATOR, Code, Message, Type);
        public static Error Deserialize(string serialized)
        {
            string[] parts = serialized.Split(SEPARATOR);

            if (parts.Length < 3)
            {
                throw new ArgumentException("Invalid serialized format");
            }

            if (Enum.TryParse<ErrorType>(parts[2], out var type) == false)
            {
                throw new ArgumentException("Invalid serialized format");
            }

            return new(parts[0], parts[1], type);
        }
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
