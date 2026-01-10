import { useState } from "react";

export function useFilterLocations() {
  const [departmentIds, setDepartmentIds] = useState<string[] | undefined>(
    undefined
  );
  const [search, setSearch] = useState<string | undefined>("");
  const [isActive, setIsActive] = useState<boolean>(true);

  return {
    departmentIds,
    setDepartmentIds,
    search,
    setSearch,
    isActive,
    setIsActive,
  };
}
