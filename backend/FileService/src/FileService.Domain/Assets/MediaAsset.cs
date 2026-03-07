using SharedKernel.Result;

namespace FileService.Domain.Assets
{
    public abstract class MediaAsset
    {
        public Guid Id { get; protected set; }

        public MediaData MediaData { get; protected set; } = null!;

        public AssetType AssetType { get; protected set; }

        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

        public StorageKey RawKey { get; protected set; } = null!;

        public StorageKey? FinalKey { get; protected set; }

        public MediaOwner Owner { get; protected set; } = null!;

        public MediaStatus Status { get; protected set; }

        protected MediaAsset()
        {
        }

        protected MediaAsset(
            Guid id,
            MediaData mediaData,
            MediaStatus status,
            AssetType assetType,
            MediaOwner owner,
            StorageKey rawKey,
            StorageKey? finalKey = null)
        {
            Id = id;
            MediaData = mediaData;
            Status = status;
            AssetType = assetType;
            Owner = owner;
            RawKey = rawKey;
            FinalKey = finalKey;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public Result MarkUploaded(DateTime uploadedAt)
        {
            if (Status == MediaStatus.UPLOADED)
            {
                return Result.Success();
            }

            if (Status == MediaStatus.UPLOADING)
            {
                Status = MediaStatus.UPLOADED;
                UpdatedAt = uploadedAt;
                return Result.Success();
            }

            return Error.Validation("status.invalid", $"Переход не допустим с текущим статусом: {Status}");
        }

        public Result MarkReady(StorageKey finalKey, DateTime readyAt)
        {
            if (Status == MediaStatus.READY)
            {
                return Result.Success();
            }

            if (Status == MediaStatus.UPLOADED)
            {
                Status = MediaStatus.READY;
                FinalKey = finalKey;
                UpdatedAt = readyAt;
                return Result.Success();
            }

            return Error.Validation("status.invalid", $"Переход не допустим с текущим статусом: {Status}");
        }

        public Result MarkFailed(DateTime failedAt)
        {
            if (Status == MediaStatus.FAILED)
            {
                return Result.Success();
            }

            if (Status == MediaStatus.UPLOADING || Status == MediaStatus.UPLOADED)
            {
                Status = MediaStatus.FAILED;
                UpdatedAt = failedAt;
                return Result.Success();
            }

            return Error.Validation("status.invalid", $"Переход не допустим с текущим статусом: {Status}");
        }

        public Result MarkDeleted(DateTime deletedAt)
        {
            if (Status == MediaStatus.DELETED)
            {
                return Result.Success();
            }

            if (Status == MediaStatus.UPLOADING ||
                Status == MediaStatus.UPLOADED ||
                Status == MediaStatus.READY ||
                Status == MediaStatus.FAILED)
            {
                Status = MediaStatus.DELETED;
                UpdatedAt = deletedAt;
                return Result.Success();
            }

            return Error.Validation("status.invalid", $"Переход не допустим с текущим статусом: {Status}");
        }
    }
}