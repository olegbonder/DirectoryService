import { AssetType, FileValidatorConfig, FileValidatorResult } from "../types";

const KB = 1024;
const MB = KB * 1024;
const GB = MB * 1024;

export const fileValidators: Record<AssetType, FileValidatorConfig> = {
  video: {
    maxFileSize: 5 * GB,
    allowedExtensions: [".mp4", ".mkv", ".avi", ".mov"],
    allowedTypes: [
      "video/mp4",
      "video/x-matroska",
      "video/quicktime",
      "video/x-msvideo",
    ],
    label: "Видео",
    description: "MP4, MKV, AVI, MOV (макс. 5 ГБ)",
  },
  preview: {
    maxFileSize: 10 * MB,
    allowedExtensions: [".jpg", ".jpeg", ".png", ".webp"],
    allowedTypes: ["image/jpeg", "image/png", "image/webp"],
    label: "Превью",
    description: "JPG, JPEG, PNG, WEBP (макс. 10 МБ)",
  },
};

export function formatFileSize(bytes: number): string {
  if (bytes < KB) return `${bytes} B`;
  if (bytes < MB) return `${(bytes / KB).toFixed(1)} KB`;
  if (bytes < GB) return `${(bytes / MB).toFixed(1)} MB`;
  return `${(bytes / GB).toFixed(2)} GB`;
}

function getFileExtension(fileName: string): string {
  const lastDot = fileName.lastIndexOf(".");
  if (lastDot === -1) return "";
  return fileName.slice(lastDot).toLowerCase();
}

export function validateFile(
  file: File,
  assetType: AssetType,
): FileValidatorResult {
  const config = fileValidators[assetType];

  if (file.size > config.maxFileSize) {
    return {
      valid: false,
      error: `Файл слишком большой. Максимальный размер: ${formatFileSize(config.maxFileSize)}.`,
    };
  }

  if (file.size === 0) {
    return {
      valid: false,
      error: "Файл не может быть пустым.",
    };
  }

  if (!config.allowedTypes.includes(file.type)) {
    return {
      valid: false,
      error: `Недопустимый тип файла. Разрешенные типы: ${config.allowedTypes.join(", ")}.`,
    };
  }

  const extension = getFileExtension(file.name);
  if (!config.allowedExtensions.includes(extension)) {
    return {
      valid: false,
      error: `Недопустимое расширение файла. Разрешенные расширения: ${config.allowedExtensions.join(", ")}.`,
    };
  }

  return { valid: true };
}

export function getAcceptString(assetType: AssetType): string {
  const config = fileValidators[assetType];
  return config.allowedExtensions.join(",");
}

export function getValidatorConfig(assetType: AssetType): FileValidatorConfig {
  return fileValidators[assetType];
}
