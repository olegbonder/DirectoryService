"use client";

import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Switch } from "@/shared/components/ui/switch";

type LocationFilterProps = {
  departmentIds?: string[];
  setDepartmentIds: (ids: string[] | undefined) => void;
  search?: string;
  setSearch: (search: string | undefined) => void;
  isActive?: boolean;
  setIsActive: (isActive: boolean) => void;
};

export default function LocationFilters({
  departmentIds,
  setDepartmentIds,
  search,
  setSearch,
  isActive,
  setIsActive,
}: LocationFilterProps) {
  return (
    <div className="flex items-center gap-4">
      <Input
        className="flex gap-2"
        placeholder="Поиск по наименованию локации"
        value={search}
        onChange={(event) => setSearch(event.target.value)}
      />
      <Switch
        id="isActive"
        checked={isActive ?? false}
        onCheckedChange={(checked) => setIsActive(checked ? true : false)}
      ></Switch>
      <Label htmlFor="isActive">Активные</Label>
    </div>
  );
}
