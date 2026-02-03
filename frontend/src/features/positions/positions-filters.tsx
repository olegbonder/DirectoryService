"use client";

import { Input } from "@/shared/components/ui/input";
import { Search } from "lucide-react";
import {
  setFilterPositionsDepartmentIds,
  setFilterPositionsIsActive,
  setFilterPositionsSearch,
  useGetPositionsFilter,
} from "./model/positions-filters-store";
import DepartmentSelect from "../departments/department-select";
import StatusSelect from "../status/status-select";

export default function PositionFilters() {
  const { search, departmentIds, isActive } = useGetPositionsFilter();

  return (
    <div className="flex items-center gap-4">
      <div className="relative flex-1">
        <Search className="absolute top-1/2 left-3 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          className="pl-10"
          placeholder="Поиск по наименованию позиции"
          value={search}
          onChange={(e) => setFilterPositionsSearch(e.target.value)}
        />
      </div>
      <DepartmentSelect
        selectedDepartmentIds={departmentIds}
        onDepartmentChange={setFilterPositionsDepartmentIds}
      />
      <StatusSelect
        isActive={isActive}
        onIsActiveChange={setFilterPositionsIsActive}
      />
    </div>
  );
}
