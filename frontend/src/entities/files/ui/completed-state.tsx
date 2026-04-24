import { CheckCircle2, File } from "lucide-react";

type Props = {
  fileName: string;
  message?: string;
  icon?: React.ReactNode;
};

export function CompletedState({ fileName, message, icon }: Props) {
  return (
    <div className="border border-green-200 bg-green-50 rounded-lg p-4">
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center shrink-0">
          {icon || <File className="w-5 h-5 text-green-600" />}
        </div>
        <div className="flex-1">
          {fileName && <p className="text-sm font-medium">{fileName}</p>}
          <div className="flex items-center gap-1.5 text-green-600">
            <CheckCircle2 className="w-4 h-4" />
            <p className="text-sm">{message || "Загрузка завершена"}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
