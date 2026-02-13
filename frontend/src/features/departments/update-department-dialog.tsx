import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Building2 } from "lucide-react";
import { DepartmentDetail } from "@/entities/departments/types";
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
import { useUpdateAndMoveDepartment } from "./model/use-update-move-department";
import {
  UpdateDepartmentData,
  updateDepartmentSchema,
} from "@/entities/departments/validations";
import DepartmentSelect from "./department-select";
import { useDepartmentDictionary } from "./model/use-department-dictionary";
import { PAGE_SIZE } from "@/shared/api/types";

type UpdateDepartmentProps = {
  department: DepartmentDetail;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function UpdateDepartmentDialog({
  department,
  open,
  onOpenChange,
}: UpdateDepartmentProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    control,
  } = useForm<UpdateDepartmentData>({
    defaultValues: {
      name: department.name,
      identifier: department.identifier,
      parentId: department.parentId,
    },
    resolver: zodResolver(updateDepartmentSchema),
    mode: "onChange",
  });

  const { updateDepartment, isPending } = useUpdateAndMoveDepartment();

  const {
    departments,
    isPending: isDepartmentsPending,
    isError: isDepartmentsError,
    error: departmentsError,
    fetchNextPage: fetchNextDepartmentsPage,
    isFetchingNextPage: isFetchingNextDepartmentsPage,
    hasNextPage: hasNextDepartmentsPage,
  } = useDepartmentDictionary({
    pageSize: PAGE_SIZE,
    showOnlyParents: false,
  });

  const onSubmit = async (data: UpdateDepartmentData) => {
    updateDepartment(
      { id: department.id, ...data },
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
            <div className="p-2 rounded-lg bg-linear-to-br from-blue-500 to-blue-600 text-white">
              <Building2 className="h-5 w-5" />
            </div>
            <DialogTitle>Редактировать подразделение</DialogTitle>
          </div>
          <DialogDescription>
            Заполните форму ниже, чтобы отредактировать подразделение.
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
                  placeholder="Введите наименование подразделения"
                  aria-invalid={errors.name ? true : false}
                  className={
                    errors.name ? "border-red-500 focus:ring-red-500" : ""
                  }
                  {...register("name")}
                />
                <FieldError>{errors.name?.message}</FieldError>
              </Field>
              <Field data-invalid={errors.identifier}>
                <FieldLabel htmlFor="identifier" data-invalid>
                  Идентификатор
                </FieldLabel>
                <Input
                  id="identifier"
                  placeholder="Введите идентификатор подразделения"
                  aria-invalid={errors.identifier ? true : false}
                  className={
                    errors.identifier ? "border-red-500 focus:ring-red-500" : ""
                  }
                  {...register("identifier")}
                />
                <FieldError>{errors.identifier?.message}</FieldError>
              </Field>
              <Field data-invalid={errors.parentId}>
                <FieldLabel htmlFor="parentId">
                  Родительское подразделение
                </FieldLabel>
                <Controller
                  name="parentId"
                  control={control}
                  render={({ field }) => (
                    <DepartmentSelect
                      departments={departments ?? []}
                      selectedId={field.value}
                      onChange={field.onChange}
                      isPending={isDepartmentsPending}
                      isError={isDepartmentsError}
                      error={departmentsError}
                      fetchNextPage={fetchNextDepartmentsPage}
                      isFetchingNextPage={isFetchingNextDepartmentsPage}
                      hasNextPage={hasNextDepartmentsPage}
                    />
                  )}
                />
                <FieldError>{errors.parentId?.message}</FieldError>
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
              className="bg-linear-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800"
            >
              {isPending ? "Редактирование..." : "Редактировать"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
