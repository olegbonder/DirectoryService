"use client";

import { Input } from "@/shared/components/ui/input";
import { Search } from "lucide-react";
import StatusSelect from "../status/status-select";
import DepartmentMultiSelect from "./department-multi-select";
import {
  setFilterDepartmentsIdentifier,
  setFilterDepartmentsIsActive,
  setFilterDepartmentsLocationIds,
  setFilterDepartmentsName,
  setFilterDepartmentsParentId,
  useGetDepartmentsFilter,
} from "./model/departments-filters-store";
import ParentDepartmentSelect from "./parent-department-select";

export default function DepartmentFilters() {
  const { name, identifier, parentId, locationIds, isActive } =
    useGetDepartmentsFilter();

  return (
    <div className="flex items-center gap-4">
      <div className="relative flex-1">
        <Search className="absolute top-1/2 left-3 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          className="pl-10"
          placeholder="Поиск по наименованию подразделения"
          value={name}
          onChange={(e) => setFilterDepartmentsName(e.target.value)}
        />
      </div>
      <div className="relative flex-1">
        <Search className="absolute top-1/2 left-3 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          className="pl-10"
          placeholder="Поиск по идентификатору подразделения"
          value={identifier}
          onChange={(e) => setFilterDepartmentsIdentifier(e.target.value)}
        />
      </div>
      <ParentDepartmentSelect
        parentId={parentId}
        setParentId={setFilterDepartmentsParentId}
      />
      <DepartmentMultiSelect
        selectedDepartmentIds={locationIds}
        onDepartmentChange={setFilterDepartmentsLocationIds}
      />

      <StatusSelect
        isActive={isActive}
        onIsActiveChange={setFilterDepartmentsIsActive}
      />
    </div>
  );
}
