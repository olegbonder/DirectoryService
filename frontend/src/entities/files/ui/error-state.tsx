import { Button } from "@/shared/components/ui/button";
import { AlertCircle } from "lucide-react";

type Props = {
  error: string;
  //onRetry: () => void;
};

export function ErrorState({ error }: Props) {
  return (
    <div className="border border-red-200 bg-red-50 rounded-lg p-4">
      <div className="flex items-center gap-3">
        <AlertCircle className="w-5 h-5 text-destructive shrink-0 mt-0.5" />
        <div className="flex-1">
          <p className="text-sm font-medium text-destructive">
            Произошла ошибка
          </p>
          <p className="text-sm font-medium text-destructive">
            {error || "Не удалось загрузить файл"}
          </p>
        </div>
        {/*<Button variant="outline" size="sm" className="mt-3" onClick={onRetry}>
          Попробовать снова
        </Button>*/}
      </div>
    </div>
  );
}
