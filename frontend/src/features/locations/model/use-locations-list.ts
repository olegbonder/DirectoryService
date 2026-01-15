import { locationsQueryOptions } from "@/entities/locations/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";
import { LocationsFilterState } from "./locations-filters-store";

export const PAGE_SIZE = 3;

export function useLocationsList(filter: LocationsFilterState) {
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
    ...locationsQueryOptions.getLocationsInfinityOptions(filter),
  });

  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (node) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage) {
            fetchNextPage();
          }
        },
        {
          threshold: 0.5,
        }
      );

      if (node) {
        observer.observe(node);
      }

      return () => observer.disconnect();
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  );

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
