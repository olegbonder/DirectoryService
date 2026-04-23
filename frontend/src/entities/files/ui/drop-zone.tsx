import { cn } from "@/shared/lib/utils";
import { Upload } from "lucide-react";

export function DropZone({
  isDragging,
  onDragOver,
  onDragLeave,
  onDrop,
  onClick,
  label = "Перетащите файл сюда",
  description = "или нажмите, чтобы загрузить",
  icon,
}: {
  isDragging: boolean;
  onDragOver: (e: React.DragEvent) => void;
  onDragLeave: (e: React.DragEvent) => void;
  onDrop: (e: React.DragEvent) => void;
  onClick?: () => void;
  label?: string;
  description?: string;
  icon?: React.ReactNode;
}) {
  return (
    <div
      className={cn(
        "border-2 border-dashed rounded-lg p-8 text-center transition-colors cursor-pointer",
        isDragging
          ? "border-primary bg-primary/50"
          : "border-muted-foreground/25 hover:border-primary/50",
      )}
      onDragOver={onDragOver}
      onDragLeave={onDragLeave}
      onDrop={onDrop}
      onClick={onClick}
    >
      {icon ?? (
        <Upload className="w-10 h-10 mx-auto mb-3 text-muted-foreground" />
      )}
      <p className="text-sm font-medium">{label}</p>
      <p className="text-xs text-muted-foreground mt-1">{description}</p>
    </div>
  );
}
