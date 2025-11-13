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
            return Error.Conflict("location.address.conflict", $"Локация с указанным адресом уже существует");
        }

        public static Error CountryIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("location.address.country", "Страна");
        }

        public static Error CityIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("location.address.city", "Город");
        }

        public static Error StreetIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("location.address.street", "Улица");
        }

        public static Error HouseNumberIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("location.address.house", "Дом");
        }

        public static Error TimezoneIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("location.timezone", "Часовой пояс");
        }

        public static Error TimezoneInvalid()
        {
            return Error.Validation("location.timezone", "Указанное значение не является часовым поясом!", "timezone");
        }

        public static Error NameIsEmpty()
        {
            return GeneralErrors.PropertyIsEmpty("location.name");
        }

        public static Error NameLengthOutOfRange(int min, int max)
        {
            return GeneralErrors.PropertyOutOfRange("location.name", min, max);
        }

        public static Error NotFound(Guid id)
        {
            return GeneralErrors.NotFound("location", id);
        }
    }
}
