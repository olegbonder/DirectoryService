using SharedKernel.Result;

namespace FileService.Domain.Shared;

public static class MediaAssetErrors
{
    public static Error DatabaseError()
    {
        return Error.Failure("media_asset.database.error", "Ошибка сохранения медиа-файла");
    }

    public static Error FileLength()
    {
        return Error.Validation("media_asset.length", "Размер медиа-файла должен быть больше 0");
    }

    public static Error FailedAssetType()
    {
        return Error.Validation("asset_type.failed", "Неправильный тип медиа-файла");
    }
}