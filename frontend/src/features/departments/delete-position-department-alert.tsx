import { useDeletePositionDepartment } from "@/features/departments/model/use-delete-position-department";
import DeleteAlertDialog from "../dialog/delete-alert-dialog";

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
  return (
    <DeleteAlertDialog
      title="Удалить подразделение?"
      message={
        <>
          Вы уверены, что хотите удалить подразделение
          <span className="font-semibold text-slate-900">
            {`"${departmentName}"`}
          </span>
          ?
        </>
      }
      isPending={isPending}
      open={open}
      onOpenChange={onOpenChange}
      onConfirm={() =>
        deleteDeparment(
          {
            positionId,
            departmentId,
          },
          { onSuccess: () => onConfirm() },
        )
      }
    />
  );
}
