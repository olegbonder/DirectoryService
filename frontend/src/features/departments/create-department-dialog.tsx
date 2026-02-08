import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Building } from "lucide-react";
import { useForm, Controller, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  FieldGroup,
  FieldSet,
  Field,
  FieldLabel,
  FieldError,
  FieldContent,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import DepartmentSelect from "../departments/department-select";
import { useCreateDepartment } from "./model/use-create-department";
import {
  CreateDepartmentData,
  createDepartmentSchema,
} from "@/entities/departments/validations";
import LocationDictionary from "../locations/location-dictionary";
import { useDepartmentsList } from "./model/use-departments-list";
import { PAGE_SIZE } from "@/shared/api/types";
import DepartmentPath from "./department-path";
import React from "react";

type CreateDepartmentProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function CreateDepartmentDialog({
  open,
  onOpenChange,
}: CreateDepartmentProps) {
  const initialData: CreateDepartmentData = {
    name: "",
    identifier: "",
    parentId: undefined,
    locationIds: [],
  };
  const {
    control,
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateDepartmentData>({
    defaultValues: initialData,
    resolver: zodResolver(createDepartmentSchema),
  });

  const { createDepartment, isPending: isCreating } = useCreateDepartment();

  const {
    departments,
    isPending: isDepartmentsPending,
    isError: isDepartmentsError,
    error: departmentsError,
    fetchNextPage: fetchNextDepartmentsPage,
    isFetchingNextPage: isFetchingNextDepartmentsPage,
    hasNextPage: hasNextDepartmentsPage,
  } = useDepartmentsList({
    pageSize: PAGE_SIZE,
  });

  // Отслеживаем изменения по parentId и identifier
  const watchedParentId = useWatch({
    control,
    name: "parentId",
  });

  const watchedIdentifier = useWatch({
    control,
    name: "identifier",
  });

  const [calculatedPath, setCalculatedPath] = useState<string>("");
  const [calculatedDepth, setCalculatedDepth] = useState<number>(0);

  React.useEffect(() => {
    if (watchedParentId && departments) {
      const selectedDepartment = departments.find(
        (dep) => dep.id === watchedParentId,
      );
      if (selectedDepartment) {
        setCalculatedPath(`${selectedDepartment.path}.${watchedIdentifier}`);
        setCalculatedDepth(selectedDepartment.path.split(".").length + 1);
      }
    } else if (watchedIdentifier) {
      setCalculatedPath(watchedIdentifier);
      setCalculatedDepth(1);
    } else {
      setCalculatedPath("");
      setCalculatedDepth(0);
    }
  }, [watchedIdentifier, watchedParentId, departments]);

  const onClose = () => {
    reset();
    onOpenChange(false);
  };

  const onSubmit = async (data: CreateDepartmentData) => {
    createDepartment(data, {
      onSuccess: () => onClose(),
    });
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <div className="flex items-center gap-2 mb-2">
            <div className="p-2 rounded-lg bg-linear-to-br from-emerald-500 to-emerald-600 text-white">
              <Building className="h-5 w-5" />
            </div>
            <DialogTitle>Создать подразделение</DialogTitle>
          </div>
          <DialogDescription>
            Заполните форму ниже, чтобы создать новое подразделение.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <FieldGroup>
            <FieldSet>
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
                  placeholder="Введите индентификатор подразделения"
                  aria-invalid={errors.identifier ? true : false}
                  className={
                    errors.identifier ? "border-red-500 focus:ring-red-500" : ""
                  }
                  {...register("identifier")}
                />
                <FieldError>{errors.identifier?.message}</FieldError>
              </Field>
              <Field>
                <FieldLabel htmlFor="path" data-invalid>
                  Путь
                </FieldLabel>
                <FieldContent>
                  {calculatedPath === "" ? (
                    <span className="text-gray-500">Нет данных</span>
                  ) : (
                    <DepartmentPath path={calculatedPath} />
                  )}
                </FieldContent>
              </Field>
              <Field>
                <div className="flex items-center justify-between">
                  <FieldLabel htmlFor="depth" data-invalid>
                    Глубина
                  </FieldLabel>
                  <div className="ml-2 ">
                    <FieldContent className="text-sm text-slate-700">
                      {calculatedDepth}
                    </FieldContent>
                  </div>
                </div>
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
              <Field data-invalid={errors.locationIds}>
                <FieldLabel htmlFor="locationIds">Локации</FieldLabel>
                <Controller
                  name="locationIds"
                  control={control}
                  render={({ field }) => (
                    <LocationDictionary onLocationChange={field.onChange} />
                  )}
                />
                <FieldError>{errors.locationIds?.message}</FieldError>
              </Field>
            </FieldSet>
          </FieldGroup>
          <DialogFooter className="pt-6 gap-3">
            <Button variant="outline" onClick={onClose}>
              Отмена
            </Button>
            <Button
              type="submit"
              className="bg-linear-to-r from-emerald-600 to-emerald-700 hover:from-emerald-700 hover:to-emerald-800"
            >
              Создать подразделение
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
