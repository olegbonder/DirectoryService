import { useQuery } from "@tanstack/react-query";
import { departmentsQueryOptions } from "@/entities/departments/api";
import { DepartmentsTreeState } from "./departments-tree-store";

export function useDepartmentRootsQuery(filter: DepartmentsTreeState) {
  const { data, isPending, error, isError, refetch } = useQuery({
    ...departmentsQueryOptions.getDepartmentRootsOptions(filter),
  });

  return {
    rootDepartments: data?.items,
    totalPages: data?.totalPages ?? 1,
    currentPage: data?.page ?? 1,
    isPending,
    error,
    isError,
    refetch,
  };
}
