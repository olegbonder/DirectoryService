using Shared.Result;
using System.Xml.Linq;

namespace DirectoryService.Domain.Shared
{
    public static class DepartmentErrors
    {
        public static Error DatabaseError()
        {
            return Error.Failure("department.database.error", "Ошибка сохранения подразделения");
        }

        public static Error OperationCancelled()
        {
            return GeneralErrors.OperationCancelled("department");
        }

        public static Error NameIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("department.name");
        }

        public static Error NameLengthOutOfRange(int min, int max)
        {
            return GeneralErrors.PropertyOutOfRange("department.name", min, max);
        }

        public static Error IdentifierIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("department.identifier", "Идентификатор");
        }

        public static Error IdentifierLengthOutOfRange(int min, int max)
        {
            return GeneralErrors.PropertyOutOfRange("department.identifier", min, max, "Идентификатор");
        }

        public static Error IdentifierMustBeLatin()
        {
            return Error.Validation(
                    "department.identifier.length.must.be.latin",
                    "Свойство \"Identifier\" должно содержать латинские буквы");
        }

        public static Error DepartmentMustHaveMoreOneLocation()
        {
            return Error.Validation("department.has.not.locations", "У подразделения должна быть хотя бы одна локация");
        }

        public static Error NotFound(Guid id)
        {
            return GeneralErrors.NotFound("department", id);
        }
    }
}
