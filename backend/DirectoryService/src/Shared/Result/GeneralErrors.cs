namespace Shared.Result
{
    public static class GeneralErrors
    {
        public static Error ValueIsRequired(string? name = null)
        {
            var label = name == null ? string.Empty : $" {name} ";
            return Error.Validation($"value.is.null", $"Поле {label} должно быть заполнено!");
        }

        public static Error NotFound(string entity, Guid? id)
        {
            var forId = id == null ? string.Empty : $" for id: '{id}'";
            return Error.NotFound($"{entity}.not.found", $"record not found{forId}");
        }

        public static Error PropertyIsEmpty(string property, string name = "Название")
        {
            return Error.Validation(
                $"{property.ToLower()}.is.empty",
                $"Свойство '{name}' не должно быть пустым");
        }

        public static Error PropertyOutOfRange(string property, int min, int max, string name = "Название")
        {
            return Error.Validation(
                $"{property.ToLower()}.length.out.of.range",
                $"Свойство '{name}' не должно быть меньше {min} или больше {max} символов");
        }

        public static Error AllreadyExists(string? entity = null)
        {
            var label = entity ?? "record";
            return Error.Validation($"{label}.already.exist", "Запись уже существует");
        }

        public static Error OperationCancelled(string property)
        {
            return Error.Failure($"{property}.operation.canceled", "Операция была отменена");
        }

        public static Error RequestIsNull()
        {
            return Error.Validation($"request.is.null", $"Запрос не может быть пустым!");
        }

        public static Error ConcurrentOperation(string property)
        {
            return Error.Conflict($"{property}.database.update.concurrent", "Другой пользователь параллельно изменил эту же запись");
        }
    }
}
