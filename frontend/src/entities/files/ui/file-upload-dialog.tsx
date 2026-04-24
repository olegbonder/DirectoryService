"use client";

import {
  Dialog,
  DialogHeader,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/ui/dialog";
import { useRef, useState } from "react";
import { useFileUpload } from "../model/use-file-upload";
import { AssetType, OwnerType } from "../types";
import { DropZone } from "./drop-zone";
import { UploadingState } from "./uploading-state";
import { CompletedState } from "./completed-state";
import { ErrorState } from "./error-state";
import { Button } from "@/shared/components/ui/button";
import { getAcceptString, getValidatorConfig } from "../lib/validators";

type Props = {
  contextId: string;
  context: OwnerType;
  assetType: AssetType;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: (mediaAssetId: string) => Promise<void>;
};
export function FileUploadDialog({
  contextId,
  context,
  assetType,
  open,
  onOpenChange,
  onSuccess,
}: Props) {
  const [isDragging, setIsDragging] = useState(false);

  const config = getValidatorConfig(assetType);
  const {
    upload,
    cancel,
    uploadState,
    isIdle,
    isUploading,
    isCompleted,
    isFailed,
    error,
  } = useFileUpload({
    contextId,
    context,
    assetType,
    onSuccess,
  });
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleFileSelected = async (file: File) => {
    await upload(file);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    const file = e.dataTransfer.files[0];
    if (file) {
      handleFileSelected(file);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    handleFileSelected(file);
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) handleFileSelected(file);

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const openFilePicker = () => fileInputRef.current?.click();

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{`Загрузка ${config.label.toLowerCase()}`}</DialogTitle>
          <DialogDescription />
        </DialogHeader>
        <input
          ref={fileInputRef}
          type="file"
          accept={getAcceptString(assetType)}
          className="hidden"
          onChange={handleFileInputChange}
          aria-label={`Выбор файла: ${config.label.toLowerCase()}`}
        />

        <div className="space-y-4">
          {isIdle && (
            <DropZone
              isDragging={isDragging}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              onClick={openFilePicker}
              label={`Перетащите ${config.label.toLowerCase()}`}
              description={config.description}
            />
          )}
          {isUploading && (
            <UploadingState
              fileName={uploadState.fileName}
              fileSize={uploadState.fileSize}
              progress={uploadState.progress}
              uploadedBytes={uploadState.uploadedBytes}
              totalBytes={uploadState.totalBytes}
              onCancel={cancel}
            />
          )}
          {isFailed && <ErrorState error={error!} />}
          {isCompleted && <CompletedState fileName={uploadState.fileName!} />}
        </div>
        <DialogFooter>
          {isIdle && (
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Отмена
            </Button>
          )}
          {isCompleted && (
            <Button onClick={() => onOpenChange(false)}>Готово</Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
