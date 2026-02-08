"use client";

import SortSelect from "../sort/sort-select";
import {
  SelectContent,
  Select,
  SelectTrigger,
  SelectValue,
  SelectItem,
} from "@/shared/components/ui/select";
import { DepartmentOrderColumnn } from "@/entities/departments/types";
import { setDepartmentsOrder, useGetDepartmentsFilter } from "./model/departments-filters-store";

type orderColumnOptions = {
  value: DepartmentOrderColumnn;
  label: string;
};

const orderColumns: orderColumnOptions[] = [
  { value: "name", label: "Наименование" },
  { value: "path", label: "Путь" },
  { value: "createdAt", label: "Дата создания" },
];

export default function DepartmentSort() {
  const { orderColumnn } = useGetDepartmentsFilter();

  return (
    <div className="flex items-center gap-4">
      <Select
        onValueChange={(value) =>
          setDepartmentsOrder({
            field: value as DepartmentOrderColumnn,
            direction: orderColumnn?.direction ?? "asc",
          })
        }
        value={orderColumnn?.field}
      >
        <SelectTrigger>
          <SelectValue placeholder="Поле" />
        </SelectTrigger>
        <SelectContent>
          {orderColumns.map((column) => (
            <SelectItem key={column.value} value={column.value}>
              {column.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <SortSelect
        order={orderColumnn?.direction ?? "asc"}
        setOrder={(value) =>
          setDepartmentsOrder({
            field: orderColumnn?.field ?? "name",
            direction: value,
          })
        }
      />
    </div>
  );
}