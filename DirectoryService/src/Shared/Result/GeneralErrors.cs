using System.Xml.Linq;

namespace Shared.Result
{
    public static class GeneralErrors
    {
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
    }
}
