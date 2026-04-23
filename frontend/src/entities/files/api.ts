import { fsApiClient } from "@/shared/api/axios-instance";
import { AssetType, OwnerType } from "./types";
import { Envelope } from "@/shared/api/envelope";
import axios from "axios";

export type StartMultipartUploadRequest = {
  fileName: string;
  assetType: AssetType;
  contentType: string;
  size: number;
  context: OwnerType;
  contextId: string;
};

export type ChunkUploadUrl = {
  partNumber: number;
  uploadUrl: string;
};

export type StartMultipartUploadResponse = {
  mediaAssetId: string;
  uploadId: string;
  chunkUploadUrls: ChunkUploadUrl[];
  chunkSize: number;
};

export type PartETag = {
  partNumber: number;
  eTag: string;
};

export type CompleteMultipartUploadRequest = {
  mediaAssetId: string;
  uploadId: string;
  partETags: PartETag[];
};

export type CompleteMultipartUploadResponse = {
  mediaAssetId: string;
};

export type AbortMultipartUploadRequest = {
  mediaAssetId: string;
  uploadId: string;
};

export const fileApi = {
  startMultipartUpload: async (
    request: StartMultipartUploadRequest,
    signal?: AbortSignal,
  ): Promise<StartMultipartUploadResponse> => {
    const response = await fsApiClient.post<
      Envelope<StartMultipartUploadResponse>
    >("/files/multipart/start", request, { signal });
    return response.data.result!;
  },
  uploadChunk: async (
    uploadUrl: string,
    chunk: Blob,
    signal?: AbortSignal,
  ): Promise<string> => {
    const response = await axios.put(uploadUrl, chunk, {
      headers: {
        "Content-Type": chunk.type,
      },
      signal,
    });

    const eTag = response.headers.etag?.replace(/"/g, "") || ""; // Удаляем кавычки из eTag
    return eTag;
  },
  completeMultipartUpload: async (
    request: CompleteMultipartUploadRequest,
    signal?: AbortSignal,
  ): Promise<CompleteMultipartUploadResponse> => {
    const response = await fsApiClient.post<
      Envelope<CompleteMultipartUploadResponse>
    >("/files/multipart/end", request, { signal });
    return response.data.result!;
  },

  abortMultipartUpload: async (
    request: AbortMultipartUploadRequest,
  ): Promise<void> => {
    const response = await fsApiClient.post<Envelope<void>>(
      "/files/multipart/cancel",
      request,
    );
    return response.data.result!;
  },
};
