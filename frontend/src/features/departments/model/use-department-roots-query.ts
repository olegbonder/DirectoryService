import { useQuery } from "@tanstack/react-query";
import {
  departmentsQueryOptions,
  GetRootDepartmentsRequest,
} from "@/entities/departments/api";

export function useDepartmentRootsQuery(filter: GetRootDepartmentsRequest) {
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
