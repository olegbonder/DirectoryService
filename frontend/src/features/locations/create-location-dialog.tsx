import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { useCreateLocation } from "./model/use-create-location";
import { MapPin } from "lucide-react";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import ChangeLocationForm from "./change-location-form";
import {
  ChangeLocationData,
  changeLocationSchema,
} from "@/entities/locations/validations";

type CreateLocationProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function CreateLocationDialog({
  open,
  onOpenChange,
}: CreateLocationProps) {
  const initialData: ChangeLocationData = {
    name: "",
    timeZone: "",
    address: {
      country: "",
      city: "",
      street: "",
      house: "",
      flat: undefined,
    },
  };

  const {
    control,
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ChangeLocationData>({
    defaultValues: initialData,
    resolver: zodResolver(changeLocationSchema),
  });

  const { createLocation, isPending } = useCreateLocation();

  const onClose = () => {
    reset(initialData);
    onOpenChange(false);
  };
  const onSubmit = async (data: ChangeLocationData) => {
    createLocation(data, {
      onSuccess: () => onClose(),
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <div className="flex items-center gap-2 mb-2">
            <div className="p-2 rounded-lg bg-gradient-to-br from-emerald-500 to-emerald-600 text-white">
              <MapPin className="h-5 w-5" />
            </div>
            <DialogTitle>Создать локацию</DialogTitle>
          </div>
          <DialogDescription>
            Заполните форму ниже, чтобы создать новую локацию.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <ChangeLocationForm
            control={control}
            register={register}
            errors={errors}
          />
          <DialogFooter className="pt-6 gap-3">
            <Button variant="outline" onClick={onClose}>
              Отмена
            </Button>
            <Button
              type="submit"
              disabled={isPending}
              className="bg-gradient-to-r from-emerald-600 to-emerald-700 hover:from-emerald-700 hover:to-emerald-800"
            >
              {isPending ? "Создание..." : "Создать локацию"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
