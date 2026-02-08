import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Briefcase } from "lucide-react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  FieldGroup,
  FieldSet,
  Field,
  FieldLabel,
  FieldError,
} from "@/shared/components/ui/field";
import { useAddDepartmentsToPosition } from "./model/use-add-departments-to-position";
import {
  AddDepartmentsToPositionData,
  addDepartmentsToPositionSchema,
} from "@/entities/departments/validations";
import DepartmentSelect from "./department-multi-select";

type AddDepartmentsToPositionProps = {
  positionId: string;
  positionDepartmentIds: string[];
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function AddDepartmentsToPositionDialog({
  positionId,
  positionDepartmentIds,
  open,
  onOpenChange,
}: AddDepartmentsToPositionProps) {
  const initialData: AddDepartmentsToPositionData = {
    departmentIds: [],
  };
  const {
    control,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<AddDepartmentsToPositionData>({
    defaultValues: initialData,
    resolver: zodResolver(addDepartmentsToPositionSchema),
  });

  const { addDepartments } = useAddDepartmentsToPosition();

  const onClose = () => {
    reset();
    onOpenChange(false);
  };

  const onSubmit = async (data: AddDepartmentsToPositionData) => {
    addDepartments(
      { positionId, departmentIds: data.departmentIds },
      {
        onSuccess: () => onClose(),
      },
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <div className="flex items-center gap-2 mb-2">
            <div className="p-2 rounded-lg bg-linear-to-br from-blue-500 to-blue-600 text-white">
              <Briefcase className="h-5 w-5" />
            </div>
            <DialogTitle>Добавить подразделения в позицию</DialogTitle>
          </div>
          <DialogDescription>
            Заполните форму ниже, чтобы добавить подразделения в позицию.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <FieldGroup>
            <FieldSet>
              <Field data-invalid={errors.departmentIds}>
                <FieldLabel htmlFor="departments">Подразделения</FieldLabel>
                <Controller
                  name="departmentIds"
                  control={control}
                  render={({ field }) => (
                    <DepartmentSelect
                      excludeDepartmentIds={positionDepartmentIds}
                      onDepartmentChange={field.onChange}
                    />
                  )}
                />
                <FieldError>{errors.departmentIds?.message}</FieldError>
              </Field>
            </FieldSet>
          </FieldGroup>
          <DialogFooter className="pt-6 gap-3">
            <Button variant="outline" onClick={onClose}>
              Отмена
            </Button>
            <Button
              type="submit"
              className="bg-linear-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800"
            >
              Добавить
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
