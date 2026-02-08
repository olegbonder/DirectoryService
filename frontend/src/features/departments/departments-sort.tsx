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
import {
  setDepartmentsOrderBy,
  setDepartmentsOrderDirection,
  useGetDepartmentsFilter,
} from "./model/departments-filters-store";

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
  const { orderBy, orderDirection } = useGetDepartmentsFilter();
  const handleOrderByChange = (value: DepartmentOrderColumnn | "none") => {
    if (value === "none") {
      setDepartmentsOrderBy(undefined);
    } else {
      setDepartmentsOrderBy(value);
    }
  };

  // Обеспечиваем, что значение всегда строка, чтобы избежать проблем с контролируемым компонентом
  const orderByValue = orderBy ?? "";

  return (
    <div className="flex items-center gap-4">
      <Select onValueChange={handleOrderByChange} value={orderByValue}>
        <SelectTrigger>
          <SelectValue placeholder="Выберите столбец" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="none">Без сортировки</SelectItem>
          {orderColumns.map((column) => (
            <SelectItem key={column.value} value={column.value}>
              {column.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <SortSelect
        order={orderDirection ?? "asc"}
        setOrder={(value) => setDepartmentsOrderDirection(value)}
      />
    </div>
  );
}
