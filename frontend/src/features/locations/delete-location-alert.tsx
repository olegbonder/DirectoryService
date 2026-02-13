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
import { Location } from "@/entities/locations/types";
import { useDeleteLocation } from "./model/use-delete-location";
import { AlertTriangle } from "lucide-react";
import DeleteAlertDialog from "../dialog/delete-alert-dialog";

type DeleteLocationDialogProps = {
  location: Location;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function DeleteLocationAlertDialog({
  location,
  open,
  onOpenChange,
}: DeleteLocationDialogProps) {
  const { deleteLocation, isPending } = useDeleteLocation();
  return (
    <DeleteAlertDialog
      title="Удалить локацию?"
      message={
        <>
          Вы уверены, что хотите удалить локацию
          <span className="font-semibold text-slate-900">
            {`"${location.name}"`}
          </span>
          ?
        </>
      }
      isPending={isPending}
      open={open}
      onOpenChange={onOpenChange}
      onConfirm={() =>
        deleteLocation(location.id, { onSuccess: () => onOpenChange(false) })
      }
    />
  );
}
