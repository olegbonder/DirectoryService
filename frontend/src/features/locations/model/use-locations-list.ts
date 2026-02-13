import { locationsQueryOptions } from "@/entities/locations/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { LocationsFilterState } from "./locations-filters-store";
import { useDebounce } from "use-debounce";
import useCursorRef from "@/shared/hooks/use-cursor-ref";

export function useLocationsList(filter: LocationsFilterState) {
  const [debouncedSearch] = useDebounce(filter.search ?? "", 500);
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
    ...locationsQueryOptions.getLocationsInfinityOptions({
      ...filter,
      search: debouncedSearch,
    }),
  });

  const cursorRef = useCursorRef({
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  });

  return {
    locations: data?.items,
    totalPages: data?.totalPages ?? undefined,
    currentPage: data?.page,
    isPending,
    error,
    isError,
    refetch,
    isFetchingNextPage,
    cursorRef,
  };
}
