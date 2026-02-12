import { useInfiniteQuery } from "@tanstack/react-query";
import { departmentsQueryOptions } from "@/entities/departments/api";
import useCursorRef from "@/shared/hooks/use-cursor-ref";

export function useDepartmentRootsInfinity() {
  const {
    data,
    isPending,
    error,
    isError,
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  } = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentRootsInfinityOptions(),
  });

  const cursorRef = useCursorRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    rootDepartments: data?.items,
    isPending,
    error,
    isError,
    cursorRef,
    hasNextPage,
    isFetchingNextPage,
  };
}
