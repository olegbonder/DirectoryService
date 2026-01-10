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
    deleteLocation(location.id);
  };
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Удалить локацию?</AlertDialogTitle>
          <AlertDialogDescription>
            Вы уверены, что хотите удалить локацию {location.name}?
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Отмена</AlertDialogCancel>
          <AlertDialogAction
            disabled={isPending}
            className="bg-destructive text-white hover:bg-destructive/90"
            onClick={handleDelete}
          >
            Удалить
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
