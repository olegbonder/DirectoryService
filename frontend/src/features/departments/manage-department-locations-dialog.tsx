import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { MapPin } from "lucide-react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import {
  FieldGroup,
  FieldSet,
  Field,
  FieldLabel,
  FieldError,
} from "@/shared/components/ui/field";
import {
  ManageDepartmentLocationsData,
  manageDepartmentLocationsSchema,
} from "@/entities/departments/validations";
import LocationDictionary from "../locations/location-dictionary";
import { DepartmentDetail } from "@/entities/departments/types";
import { useManageDepartmentLocation } from "./model/use-manage-department-location";

type ManageDepartmentLocationsProps = {
  department: DepartmentDetail;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function ManageDepartmentLocationsDialog({
  department,
  open,
  onOpenChange,
}: ManageDepartmentLocationsProps) {
  const initialData: ManageDepartmentLocationsData = {
    locationIds: department.locations.map((loc) => loc.id) || [],
  };

  const {
    control,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ManageDepartmentLocationsData>({
    defaultValues: initialData,
    resolver: zodResolver(manageDepartmentLocationsSchema),
  });

  // Reset form when department prop changes
  useEffect(() => {
    reset({
      locationIds: department.locations.map((loc) => loc.id) || [],
    });
  }, [department, reset]);

  const { updateLocations } = useManageDepartmentLocation();

  const onClose = () => {
    reset();
    onOpenChange(false);
  };

  const onSubmit = async (data: ManageDepartmentLocationsData) => {
    updateLocations(
      {
        departmentId: department.id,
        ...data,
      },
      {
        onSuccess: () => {
          onClose();
        },
      },
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <div className="flex items-center gap-2 mb-2">
            <div className="p-2 rounded-lg bg-linear-to-br from-green-500 to-green-600 text-white">
              <MapPin className="h-5 w-5" />
            </div>
            <DialogTitle>Управление локациями подразделения</DialogTitle>
          </div>
          <DialogDescription>
            Выберите локации, связанные с этим подразделением.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <FieldGroup>
            <FieldSet>
              <Field data-invalid={errors.locationIds}>
                <FieldLabel htmlFor="locationIds">Локации</FieldLabel>
                <Controller
                  name="locationIds"
                  control={control}
                  render={({ field }) => (
                    <LocationDictionary
                      selectedLocationIds={field.value}
                      onLocationChange={field.onChange}
                    />
                  )}
                />
                <FieldError>{errors.locationIds?.message}</FieldError>
              </Field>
            </FieldSet>
          </FieldGroup>
          <DialogFooter className="pt-6 gap-3">
            <Button type="button" variant="outline" onClick={onClose}>
              Отмена
            </Button>
            <Button
              type="submit"
              className="bg-linear-to-r from-green-600 to-green-700 hover:bg-linear-to-r hover:from-green-700 hover:to-green-800"
            >
              Сохранить
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
