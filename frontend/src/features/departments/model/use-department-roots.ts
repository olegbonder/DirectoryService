import { useInfiniteQuery } from "@tanstack/react-query";
import { departmentsQueryOptions } from "@/entities/departments/api";

export function useDepartmentRoots() {
  const { data, isPending, error, isError } = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentRootsInfinityOptions(),
  });

  return {
    rootDepartments: data?.items,
    isPending,
    error,
    isError,
  };
}
