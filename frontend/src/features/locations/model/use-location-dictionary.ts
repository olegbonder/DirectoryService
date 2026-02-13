import { locationsQueryOptions } from "@/entities/locations/api";
import { LocationDictionaryState } from "@/entities/locations/types";
import { useInfiniteQuery } from "@tanstack/react-query";

export function useLocationDictionary(filter: LocationDictionaryState) {
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
    ...locationsQueryOptions.getLocationDictionaryInfinityOptions({
      ...filter,
    }),
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
    hasNextPage,
    fetchNextPage,
  };
}
