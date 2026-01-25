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
import { AlertTriangle } from "lucide-react";
import { useDeletePositionDepartment } from "@/features/departments/model/use-delete-position-department";

type DeletePositionDepartmentDialogProps = {
  positionId: string;
  departmentId: string;
  departmentName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export default function DeletePositionDepartmentAlertDialog({
  positionId,
  departmentId,
  departmentName,
  open,
  onOpenChange,
  onConfirm,
}: DeletePositionDepartmentDialogProps) {
  const { deleteDeparment, isPending } = useDeletePositionDepartment();
  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    deleteDeparment(
      {
        positionId,
        departmentId,
      },
      { onSuccess: () => onConfirm() },
    );
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
            Вы уверены, что хотите удалить подразделение
            <span className="font-semibold text-slate-900">
              {`"${departmentName}"`}
            </span>
            ? Это действие невозможно отменить.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter className="gap-3">
          <AlertDialogCancel>Отмена</AlertDialogCancel>
          <AlertDialogAction
            className="bg-red-600 text-white hover:bg-red-700"
            onClick={handleDelete}
            disabled={isPending}
          >
            {isPending ? "Удаление..." : "Удалить"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
