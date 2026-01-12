import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Location } from "@/entities/locations/types";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import ChangeLocationForm from "./change-location-form";
import {
  ChangeLocationData,
  changeLocationSchema,
} from "@/entities/locations/validations";
import { useUpdateLocation } from "./model/use-update-location";

type UpdateLocationProps = {
  location: Location;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function UpdateLocationDialog({
  location,
  open,
  onOpenChange,
}: UpdateLocationProps) {
  const {
    control,
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ChangeLocationData>({
    defaultValues: {
      name: location.name,
      timeZone: location.timeZone,
      address: {
        country: location.country,
        city: location.city,
        street: location.street,
        house: location.house,
        flat: location.flat,
      },
    },
    resolver: zodResolver(changeLocationSchema),
    mode: "onChange",
  });

  const { updateLocation, isPending } = useUpdateLocation();

  const onSubmit = async (data: ChangeLocationData) => {
    updateLocation(
      { id: location.id, ...data },
      {
        onSuccess: () => {
          onOpenChange(false);
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Редактировать локацию</DialogTitle>
          <DialogDescription>
            Заполните форму ниже, чтобы отредактировать локацию.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <ChangeLocationForm
            control={control}
            register={register}
            errors={errors}
          />
          <DialogFooter className="pt-6">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Отмена
            </Button>
            <Button type="submit" disabled={isPending}>
              Редактировать локацию
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
