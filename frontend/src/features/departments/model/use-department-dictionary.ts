import { departmentsQueryOptions } from "@/entities/departments/api";
import { DepartmentDictionaryState } from "@/entities/departments/types";
import { useInfiniteQuery } from "@tanstack/react-query";

export function useDepartmentDictionary(filter: DepartmentDictionaryState) {
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
    ...departmentsQueryOptions.getDepartmentDictionaryInfinityOptions({
      ...filter,
    }),
  });
  return {
    departments: data?.items,
    totalPages: data?.totalPages ?? undefined,
    currentPage: data?.page,
    isPending,
    error,
    isError,
    refetch,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
  };
}
