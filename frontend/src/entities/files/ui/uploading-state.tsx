import { File } from "lucide-react";
import { formatFileSize } from "../lib/validators";
import { Progress } from "@/shared/components/ui/progress";
import { Button } from "@/shared/components/ui/button";

type Props = {
  fileName?: string;
  fileSize?: number;
  progress: number;
  uploadedBytes?: number;
  totalBytes?: number;
  onCancel?: () => void;
  icon?: React.ReactNode;
};
export function UploadingState({
  fileName,
  fileSize,
  progress,
  uploadedBytes,
  totalBytes,
  onCancel,
  icon,
}: Props) {
  const progressText =
    uploadedBytes != undefined && totalBytes
      ? `${formatFileSize(uploadedBytes)} / ${formatFileSize(totalBytes)}`
      : `${formatFileSize(fileSize ?? 0)}%`;
  return (
    <div className="border rounded-lg p-4 space-y-3">
      <div className="flex items-start gap-3">
        <div className="w-10 h-10 bg-primary/10 rounded-lg flex items-center justify-center shrink-0">
          {icon ?? <File className="w-5 h-5 text-primary" />}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium truncate">{fileName}</p>
          <p className="text-sm text-muted-foreground">{progressText}</p>
        </div>
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8 shrink-0"
          onClick={onCancel}
        >
          Отменить
        </Button>
        <div className="space-y-1.5">
          <Progress value={progress} className="h-2" />
          <div className="flex justify-between text-xs text-muted-foreground">
            <span>Загрузка...</span>
            <span>{progress}%</span>
          </div>
        </div>
      </div>
    </div>
  );
}
