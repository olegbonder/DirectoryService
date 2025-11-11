using Shared.Result;

namespace DirectoryService.Domain.Shared
{
    public static class LocationErrors
    {
        public static Error DatabaseError()
        {
            return Error.Failure("location.database.error", "Ошибка сохранения локации");
        }

        public static Error NameConflict(string name)
        {
            return Error.Conflict("location.title.conflict", $"Локация с наименованием {name} уже существует");
        }

        public static Error AddressConflict()
        {
            return Error.Conflict("location.address.conflict", $"Локация с указнным адресом уже существует");
        }
    }
}
