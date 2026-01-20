"use client";

import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  OrderBy,
  setFilterIsActive,
  setFilterSearch,
  setOrder,
  useGetLocationsFilter,
} from "./model/locations-filters-store";
import { Search } from "lucide-react";

const statuses = [
  { value: "all", label: "Все" },
  { value: "active", label: "Активные" },
  { value: "deleted", label: "Удаленные" },
];

const orderByOptions = [
  { value: "asc", label: "По возрастанию" },
  { value: "desc", label: "По убыванию" },
];

export default function LocationFilters() {
  const { search, departmentIds, isActive, order } = useGetLocationsFilter();

  const handleIsActiveChange = (value: string) => {
    if (value === "all") {
      setFilterIsActive(undefined);
    } else if (value === "active") {
      setFilterIsActive(true);
    } else {
      setFilterIsActive(false);
    }
  };

  return (
    <div className="flex items-center gap-4">
      <div className="relative flex-1">
        <Search className="absolute top-1/2 left-3 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          className="pl-10"
          placeholder="Поиск по наименованию локации"
          value={search}
          onChange={(e) => setFilterSearch(e.target.value)}
        />
      </div>

      <Select
        onValueChange={(value) => handleIsActiveChange(value)}
        value={isActive === undefined ? "all" : isActive ? "active" : "deleted"}
      >
        <SelectTrigger>
          <SelectValue placeholder="Статус" />
        </SelectTrigger>
        <SelectContent>
          {statuses.map((status) => (
            <SelectItem key={status.value} value={status.value}>
              {status.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <Select onValueChange={(value: OrderBy) => setOrder(value)} value={order}>
        <SelectTrigger>
          <SelectValue placeholder="Cортировка" />
        </SelectTrigger>
        <SelectContent>
          {orderByOptions.map((order) => (
            <SelectItem key={order.value} value={order.value}>
              {order.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
