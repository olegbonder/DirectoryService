import { Position } from "@/entities/positions/types";
import { useDeletePosition } from "./model/use-delete-position";
import DeleteAlertDialog from "../dialog/delete-alert-dialog";

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

  return (
    <DeleteAlertDialog
      title="Удалить позицию?"
      message={
        <>
          Вы уверены, что хотите удалить позицию
          <span className="font-semibold text-slate-900">
            {`"${position.name}"`}
          </span>
          ?
        </>
      }
      isPending={isPending}
      open={open}
      onOpenChange={onOpenChange}
      onConfirm={() =>
        deletePosition(position.id, {
          onSuccess: () => onConfirm(),
        })
      }
    />
  );
}
