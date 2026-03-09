using SharedKernel.Result;

namespace FileService.Domain.Shared;

public static class MediaAssetErrors
{
    public static Error DatabaseError()
    {
        return Error.Failure("media_asset.database.error", "Ошибка сохранения медиа-файла");
    }
}