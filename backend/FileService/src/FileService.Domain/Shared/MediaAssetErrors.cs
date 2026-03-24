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

    public static Error FailedContext()
    {
        return Error.Validation("context.failed", "Неправильный тип контекста медиа-файла");
    }

    public static Error Context()
    {
        return Error.Validation("context.failed", "Неправильный тип контекста медиа-файла");
    }

    public static Error MediaAssetIdNotBeNull()
    {
        return GeneralErrors.ValueIsRequired("MediaAssetId");
    }

    public static Error MediaAssetIdNotBeEmpty()
    {
        return GeneralErrors.PropertyIsEmpty("MediaAssetId", "медиа-файл");
    }

    public static Error UploadIdNotBeNull()
    {
        return GeneralErrors.ValueIsRequired("UploadId");
    }

    public static Error UploadIdNotBeEmpty()
    {
        return GeneralErrors.PropertyIsEmpty("UploadId", "Идентификатор загрузки медиа-файла");
    }

    public static Error PartNumberMustBePositive()
    {
        return Error.Validation("partNumber", "Номер чанка должен быть положительным");
    }

    public static Error PartETagsСountMustBePositive()
    {
        return Error.Validation("partETags.count", "Количество чанков должно быть положительным");
    }

    public static Error ExpectedChunksCount()
    {
        return Error.Validation("expected.chunks.count", "PartETags count must be equal to expected chunks count.");
    }

    public static Error MediaAssetIdsNotBeNull()
    {
        return GeneralErrors.ValueIsRequired("mediaAssetIds");
    }

    public static Error MediaAssetIdsNotBeEmpty()
    {
        return Error.Validation("mediaAssetIds.not.empty", "Список медиа-файлов не может быть пустым");
    }

    public static Error MediaAssetIdsMustBeUnique()
    {
        return Error.Validation("mediaAssetIds.must.be.unique", "Список медиа-файлов должен быть уникальным");
    }
}