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

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    deleteLocation(location.id, { onSuccess: () => onOpenChange(false) });
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
              Удалить локацию?
            </AlertDialogTitle>
          </div>
          <AlertDialogDescription className="text-base">
            Вы уверены, что хотите удалить локацию{" "}
            <span className="font-semibold text-slate-900">
              {`"${location.name}"`}
            </span>
            ? Это действие невозможно отменить.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter className="gap-3">
          <AlertDialogCancel>Отмена</AlertDialogCancel>
          <AlertDialogAction
            disabled={isPending}
            className="bg-red-600 text-white hover:bg-red-700"
            onClick={handleDelete}
          >
            {isPending ? "Удаление..." : "Удалить"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
