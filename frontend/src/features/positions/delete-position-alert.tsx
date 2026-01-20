import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";
import { Position } from "@/entities/positions/types";
import { AlertTriangle } from "lucide-react";
import { useDeletePosition } from "./model/use-delete-position";

type DeletePositionDialogProps = {
  position: Position;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export default function DeletePositionAlertDialog({
  position,
  open,
  onOpenChange,
  onConfirm,
}: DeletePositionDialogProps) {
  const { deletePosition, isPending } = useDeletePosition();
  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    deletePosition(position.id, { onSuccess: () => onConfirm() });
  };

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent className="sm:max-w-lg">
        <AlertDialogHeader>
          <div className="flex items-center gap-3 mb-2">
            <div className="p-2 rounded-lg bg-red-100">
              <AlertTriangle className="h-5 w-5 text-red-600" />
            </div>
            <AlertDialogTitle className="text-lg">
              Удалить позицию?
            </AlertDialogTitle>
          </div>
          <AlertDialogDescription className="text-base">
            Вы уверены, что хотите удалить позицию
            <span className="font-semibold text-slate-900">
              {`"${position.name}"`}
            </span>
            ? Это действие невозможно отменить.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter className="gap-3">
          <AlertDialogCancel>Отмена</AlertDialogCancel>
          <AlertDialogAction
            className="bg-red-600 text-white hover:bg-red-700"
            onClick={handleDelete}
          >
            Удалить
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
