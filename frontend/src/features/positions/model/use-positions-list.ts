import { positionsQueryOptions } from "@/entities/positions/api";
import { useQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import { PositionsFilterState } from "./positions-filters-store";

export const PAGE_SIZE = 4;

export function usePositionsList(filter: PositionsFilterState) {
  const [debouncedSearch] = useDebounce(filter.search ?? "", 500);
  const { data, isPending, error, isError, refetch } = useQuery({
    ...positionsQueryOptions.getPositionsOptions({
      ...filter,
      search: debouncedSearch,
    }),
  });

  return {
    positions: data?.items,
    totalPages: data?.totalPages ?? 1,
    currentPage: data?.page ?? 1,
    isPending,
    error,
    isError,
    refetch,
  };
}
