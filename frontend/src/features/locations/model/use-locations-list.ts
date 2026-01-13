import { locationsQueryOptions } from "@/entities/locations/api";
import { GetLocationsRequest } from "@/entities/locations/types";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";

const PAGE_SIZE = 3;

type LocationFilterProps = {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
};

export function useLocationsList({
  departmentIds,
  search,
  isActive,
}: LocationFilterProps) {
  const {
    data,
    isPending,
    error,
    isError,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery(
    locationsQueryOptions.getLocationsInfinityOptions({
      departmentIds,
      search,
      isActive,
      pageSize: PAGE_SIZE,
    })
  );

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
