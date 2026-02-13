"use client";

import { Input } from "@/shared/components/ui/input";
import {
  setFilterLocationsDepartmentIds,
  setFilterLocationsIsActive,
  setFilterLocationsSearch,
  setLocationsOrder,
  useGetLocationsFilter,
} from "./model/locations-filters-store";
import { Search } from "lucide-react";
import StatusSelect from "../status/status-select";
import SortSelect from "../sort/sort-select";
import DepartmentDictionary from "../departments/department-dictionary";

export default function LocationFilters() {
  const { search, departmentIds, isActive, order } = useGetLocationsFilter();

  return (
    <div className="flex items-center gap-4">
      <div className="relative flex-1">
        <Search className="absolute top-1/2 left-3 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          className="pl-10"
          placeholder="Поиск по наименованию локации"
          value={search}
          onChange={(e) => setFilterLocationsSearch(e.target.value)}
        />
      </div>
      <DepartmentDictionary
        selectedDepartmentIds={departmentIds}
        onDepartmentChange={setFilterLocationsDepartmentIds}
      />

      <StatusSelect
        isActive={isActive}
        onIsActiveChange={setFilterLocationsIsActive}
      />
      <SortSelect order={order} setOrder={setLocationsOrder} />
    </div>
  );
}
