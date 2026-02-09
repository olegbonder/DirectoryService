import { useInfiniteQuery } from "@tanstack/react-query";
import { departmentsQueryOptions } from "@/entities/departments/api";

export function useDepartmentChildren(parentId: string) {
  const { data, isPending, error, isError } = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentChildrenInfinityOptions(parentId),
  });

  return {
    childDepartments: data?.items,
    isPending,
    error,
    isError,
  };
}
