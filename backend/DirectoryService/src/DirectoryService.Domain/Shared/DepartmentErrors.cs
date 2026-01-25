using Shared.Result;

namespace DirectoryService.Domain.Shared
{
    public static class DepartmentErrors
    {
        public static Error DatabaseError()
        {
            return Error.Failure("department.database.error", "Ошибка сохранения подразделения");
        }

        public static Error DatabaseUpdateError(Guid id)
        {
            return Error.Failure("department.database.error", $"Ошибка обновления подразделения {id}");
        }

        public static Error DatabaseUpdateLocationsError(Guid id)
        {
            return Error.Failure("department.locations.database.error", $"Ошибка обновления локаций у подразделения {id}");
        }

        public static Error DatabaseUpdatePositionsError(Guid id)
        {
            return Error.Failure("department.positions.database.error", $"Ошибка обновления позиций у подразделения {id}");
        }

        public static Error DatabaseUpdateChildrenError(Guid id)
        {
            return Error.Failure("department.children.database.error", $"Ошибка обновления детей у подразделения {id}");
        }

        public static Error ParentIdConflict()
        {
            return Error.Failure("department.parent_id.conflict", $"Идентификатор родителя не должен совпадать с идентификатором подразделения");
        }

        public static Error ParentIdAsChildConflict(Guid id)
        {
            return Error.Failure("department.parent_id.as.child.conflict", $"Нельзя выбрать родителем своё \"дочернее\" подразделение.id={id}");
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

        public static Error LocationIdsNotBeNull()
        {
            return GeneralErrors.ValueIsRequired("department.locationIds");
        }

        public static Error LocationIdsNotBeEmpty()
        {
            return Error.Validation("department.locationIds.not.empty", "Список локаций не может быть пустым");
        }

        public static Error LocationIdsMustBeUnique()
        {
            return Error.Validation("department.locationIds.must.be.unique", "Список локаций должен быть уникальным");
        }

        public static Error DepartmentIdNotBeNull()
        {
            return GeneralErrors.ValueIsRequired("departmentId");
        }

        public static Error DepartmentIdNotBeEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("departmentId", "Подразделение");
        }

        public static Error NotFound(Guid id)
        {
            return GeneralErrors.NotFound("department", id);
        }

        public static Error NotFounds()
        {
            return GeneralErrors.NotFound("departments", null);
        }
    }
}
