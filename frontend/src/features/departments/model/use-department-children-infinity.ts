import { useInfiniteQuery } from "@tanstack/react-query";
import { departmentsQueryOptions } from "@/entities/departments/api";
import useCursorRef from "@/shared/hooks/use-cursor-ref";

export function useDepartmentChildrenInfinity(parentId: string) {
  const {
    data,
    isPending,
    error,
    isError,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentChildrenInfinityOptions(parentId),
  });

  const cursorRef = useCursorRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    childDepartments: data?.items,
    isPending,
    error,
    isError,
    cursorRef,
    isFetchingNextPage,
  };
}
