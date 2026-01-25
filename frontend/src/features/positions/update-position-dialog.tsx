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
import { PositionDetail } from "@/entities/positions/types";
import { useForm } from "react-hook-form";
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
import { useUpdatePosition } from "./model/use-update-position";
import {
  UpdatePositionFormData,
  updatePositionSchema,
} from "@/entities/positions/validations";

type UpdatePositionProps = {
  position: PositionDetail;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function UpdatePositionDialog({
  position,
  open,
  onOpenChange,
}: UpdatePositionProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdatePositionFormData>({
    defaultValues: {
      name: position.name,
      description: position.description,
    },
    resolver: zodResolver(updatePositionSchema),
    mode: "onChange",
  });

  const { updatePosition, isPending } = useUpdatePosition();

  const onSubmit = async (data: UpdatePositionFormData) => {
    updatePosition(
      { id: position.id, ...data },
      {
        onSuccess: () => {
          onOpenChange(false);
        },
      },
    );
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <div className="flex items-center gap-2 mb-2">
            <div className="p-2 rounded-lg bg-linear-to-br from-purple-500 to-purple-600 text-white">
              <Briefcase className="h-5 w-5" />
            </div>
            <DialogTitle>Редактировать позицию</DialogTitle>
          </div>
          <DialogDescription>
            Заполните форму ниже, чтобы отредактировать позицию.
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
            </FieldSet>
          </FieldGroup>
          <DialogFooter className="pt-6 gap-3">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Отмена
            </Button>
            <Button
              type="submit"
              className="bg-linear-to-r from-purple-600 to-purple-700 hover:from-purple-700 hover:to-purple-800"
            >
              Редактировать позицию
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
