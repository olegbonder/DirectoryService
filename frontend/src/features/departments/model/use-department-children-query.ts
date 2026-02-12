import { useQuery } from "@tanstack/react-query";
import { departmentsQueryOptions } from "@/entities/departments/api";

export function useDepartmentChildrenQuery(parentId: string, page: number) {
  const { data, isPending, error, isError, refetch } = useQuery({
    ...departmentsQueryOptions.getDepartmentChildrenOptions(parentId, page),
  });

  return {
    childDepartments: data?.items,
    totalPages: data?.totalPages ?? 1,
    currentPage: data?.page ?? 1,
    isPending,
    error,
    isError,
    refetch,
  };
}
