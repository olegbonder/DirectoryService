import { useRef, useState } from "react";
import { AssetType, OwnerType, UploadProgress } from "../types";
import {
  AbortMultipartUploadRequest,
  ChunkUploadUrl,
  fileApi,
  PartETag,
  StartMultipartUploadResponse,
} from "../api";
import { isEnvelopeError } from "@/shared/api/errors";
import { validateFile } from "../lib/validators";

export type Props = {
  contextId: string;
  context: OwnerType;
  assetType: AssetType;
  onSuccess?: (mediaAssetId: string) => Promise<void>;
};

export function useFileUpload({
  contextId,
  context,
  assetType,
  onSuccess,
}: Props) {
  const abortControllerRef = useRef<AbortController | null>(null);
  const currentUploadRef = useRef<AbortMultipartUploadRequest | null>(null);
  const [uploadState, setUploadState] = useState<UploadProgress>({
    status: "idle",
    progress: 0,
    uploadedBytes: 0,
    totalBytes: 0,
  });

  const upload = async (file: File): Promise<string | undefined> => {
    const validation = validateFile(file, assetType);
    if (!validation.valid) {
      setUploadState({
        status: "failed",
        progress: 0,
        uploadedBytes: 0,
        totalBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
        error: validation.error,
      });
      return undefined;
    }
    try {
      setUploadState({
        status: "uploading",
        progress: 0,
        uploadedBytes: 0,
        totalBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
      });

      abortControllerRef.current = new AbortController();
      const signal = abortControllerRef.current.signal;

      const uploadData: StartMultipartUploadResponse =
        await fileApi.startMultipartUpload(
          {
            fileName: file.name,
            contentType: file.type,
            size: file.size,
            assetType,
            context,
            contextId,
          },
          signal,
        );
      const { mediaAssetId, uploadId, chunkUploadUrls, chunkSize } = uploadData;

      currentUploadRef.current = { mediaAssetId, uploadId };

      const partETags = await uploadChunks(
        file,
        chunkUploadUrls,
        chunkSize,
        signal,
      );

      await fileApi.completeMultipartUpload(
        {
          mediaAssetId,
          uploadId: uploadData.uploadId,
          partETags,
        },
        signal,
      );

      setUploadState((prev) => ({
        ...prev,
        status: "completed",
        progress: 100,
        uploadedBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
      }));

      if (onSuccess) {
        await onSuccess(mediaAssetId);
      }

      return mediaAssetId;
    } catch (error) {
      const errorMessage = isEnvelopeError(error)
        ? error.firstMessage
        : error instanceof Error
          ? error.message
          : "Ошибка загрузки файла";

      const current = currentUploadRef.current;
      if (current) {
        try {
          currentUploadRef.current = null;
          await fileApi.abortMultipartUpload(current);
        } catch (e) {}
      }

      currentUploadRef.current = null;
      setUploadState((prev) => ({
        ...prev,
        status: "failed",
        error: errorMessage,
      }));
      return undefined;
    }
  };

  const uploadChunks = async (
    file: File,
    chunks: ChunkUploadUrl[],
    chunkSize: number,
    signal: AbortSignal,
  ): Promise<PartETag[]> => {
    const partETags: PartETag[] = [];

    for (let i = 0; i < chunks.length; i++) {
      const chunkInfo = chunks[i];
      const start = i * chunkSize;
      const end = Math.min(start + chunkSize, file.size);

      const chunk = file.slice(start, end);
      const eTag = await fileApi.uploadChunk(
        chunkInfo.uploadUrl,
        chunk,
        signal,
      );
      partETags.push({
        partNumber: chunkInfo.partNumber,
        eTag,
      });

      const progress = Math.round(((i + 1) / chunks.length) * 100);
      const uploadedBytes = Math.min((i + 1) * chunkSize, file.size);

      setUploadState((prev) => ({
        ...prev,
        progress,
        uploadedBytes,
      }));
    }

    return partETags;
  };

  const cancel = async () => {
    abortControllerRef.current?.abort();

    const current = currentUploadRef.current;
    if (current) {
      try {
        await fileApi.abortMultipartUpload(current);
      } catch (e) {
        console.error(e);
      }
    }

    currentUploadRef.current = null;

    setUploadState({
      status: "idle",
      progress: 0,
      uploadedBytes: 0,
      totalBytes: 0,
      fileName: "",
      fileSize: 0,
    });
  };

  return {
    upload,
    cancel,
    uploadState,
    isIdle: uploadState.status === "idle",
    isUploading: uploadState.status === "uploading",
    isCompleted: uploadState.status === "completed",
    isFailed: uploadState.status === "failed",
    error: uploadState.error,
  };
}
