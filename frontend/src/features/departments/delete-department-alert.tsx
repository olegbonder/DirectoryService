import DeleteAlertDialog from "../dialog/delete-alert-dialog";
import { Department } from "@/entities/departments/types";
import { useDeleteDepartment } from "./model/use-delete-department";

type DeleteDepartmentDialogProps = {
  department: Department;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export default function DeleteDepartmentAlertDialog({
  department,
  open,
  onOpenChange,
  onConfirm,
}: DeleteDepartmentDialogProps) {
  const { deleteDepartment, isPending } = useDeleteDepartment();

  return (
    <DeleteAlertDialog
      title="Удалить подразделение?"
      message={
        <>
          Вы уверены, что хотите удалить подразделение
          <span className="font-semibold text-slate-900">
            {`"${department.name}"`}
          </span>
          ?
        </>
      }
      isPending={isPending}
      open={open}
      onOpenChange={onOpenChange}
      onConfirm={() =>
        deleteDepartment(department.id, {
          onSuccess: () => onConfirm(),
        })
      }
    />
  );
}
