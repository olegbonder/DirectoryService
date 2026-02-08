import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import { departmentsQueryOptions } from "@/entities/departments/api";
import { DepartmentsFilterState } from "./departments-filters-store";
import useCursorRef from "@/shared/hooks/use-cursor-ref";

export function useDepartmentsList(filter: DepartmentsFilterState) {
  const [debouncedName] = useDebounce(filter.name ?? "", 500);
  const [debouncedIdentifier] = useDebounce(filter.identifier ?? "", 500);

  const {
    data,
    isPending,
    error,
    isError,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...departmentsQueryOptions.getDepartmentsInfinityOptions({
      ...filter,
      name: debouncedName,
      identifier: debouncedIdentifier,
    }),
  });

  const cursorRef = useCursorRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    departments: data?.items,
    totalPages: data?.totalPages ?? 1,
    currentPage: data?.page ?? 1,
    isPending,
    error,
    isError,
    refetch,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    cursorRef,
  };
}
