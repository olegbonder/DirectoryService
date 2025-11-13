using Shared.Result;

namespace DirectoryService.Domain.Shared
{
    public static class PositionErrors
    {
        public static Error DatabaseError()
        {
            return Error.Failure("position.database.error", "Ошибка сохранения позиции");
        }

        public static Error OperationCancelled()
        {
            return GeneralErrors.OperationCancelled("position");
        }

        public static Error NameConflict(string name)
        {
            return Error.Conflict("position.title.conflict", $"Позиция с наименованием {name} уже существует");
        }

        public static Error NameIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("position.name");
        }

        public static Error NameLengthOutOfRange(int min, int max)
        {
            return GeneralErrors.PropertyOutOfRange("position.name", min, max);
        }

        public static Error DescriptionLengthOutOfRange(int max)
        {
            return Error.Validation(
                    "position.desription.must.be.less",
                    $"Свойство 'Описание' не должно быть быть больше {max} символов");
        }

        public static Error PositionMustHaveMoreOneDepartment()
        {
            return Error.Validation("position.has.not.departments", "У позиции должно быть хотя бы одно подразделение");
        }

        public static Error ActivePositionHaveSameName()
        {
            return Error.Conflict("position.name.is.conflict", "активная должность с данным наименованием уже существует");
        }
    }
}
