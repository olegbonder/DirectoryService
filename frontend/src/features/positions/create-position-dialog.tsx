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
  FieldLegend,
  Field,
  FieldLabel,
  FieldError,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import {
  CreatePositionData,
  createPositionSchema,
} from "@/entities/positions/validations";
import { useCreatePosition } from "./model/use-create-position";
import DepartmentSelect from "../departments/department-select";

type CreatePositionProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function CreatePositionDialog({
  open,
  onOpenChange,
}: CreatePositionProps) {
  const initialData: CreatePositionData = {
    name: "",
    description: undefined,
    departmentIds: [],
  };
  const {
    control,
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreatePositionData>({
    defaultValues: initialData,
    resolver: zodResolver(createPositionSchema),
  });

  const { createPosition, isPending: isCreating } = useCreatePosition();

  const onClose = () => {
    reset();
    onOpenChange(false);
  };

  const onSubmit = async (data: CreatePositionData) => {
    createPosition(data, {
      onSuccess: () => onClose(),
    });
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <div className="flex items-center gap-2 mb-2">
            <div className="p-2 rounded-lg bg-linear-to-br from-blue-500 to-blue-600 text-white">
              <Briefcase className="h-5 w-5" />
            </div>
            <DialogTitle>Создать позицию</DialogTitle>
          </div>
          <DialogDescription>
            Заполните форму ниже, чтобы создать новую позицию.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <FieldGroup>
            <FieldSet>
              <FieldLegend>Основные данные</FieldLegend>
              <Field data-invalid={errors.name}>
                <FieldLabel htmlFor="name" data-invalid>
                  Наименование
                </FieldLabel>
                <Input
                  id="name"
                  placeholder="Введите наименование позиции"
                  aria-invalid={errors.name ? true : false}
                  className={
                    errors.name ? "border-red-500 focus:ring-red-500" : ""
                  }
                  {...register("name")}
                />
                <FieldError>{errors.name?.message}</FieldError>
              </Field>
              <Field data-invalid={errors.description}>
                <FieldLabel htmlFor="description" data-invalid>
                  Описание
                </FieldLabel>
                <Input
                  id="description"
                  placeholder="Введите описание позиции"
                  aria-invalid={errors.description ? true : false}
                  className={
                    errors.description
                      ? "border-red-500 focus:ring-red-500"
                      : ""
                  }
                  {...register("description")}
                />
                <FieldError>{errors.description?.message}</FieldError>
              </Field>
              <Field data-invalid={errors.departmentIds}>
                <FieldLabel htmlFor="departments">Подразделения</FieldLabel>
                <Controller
                  name="departmentIds"
                  control={control}
                  render={({ field }) => (
                    <DepartmentSelect onDepartmentChange={field.onChange} />
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
              Создать позицию
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
