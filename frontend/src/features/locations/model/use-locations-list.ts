import { locationsQueryOptions } from "@/entities/locations/api";
import { GetLocationsRequest } from "@/entities/locations/types";
import { useQuery } from "@tanstack/react-query";

export function useLocationsList(request: GetLocationsRequest) {
  const { data, isPending, error, isError } = useQuery(
    locationsQueryOptions.getLocationsOptions(request)
  );

  return {
    locations: data?.items,
    totalPages: data?.totalPages ?? undefined,
    currentPage: data?.page,
    isPending,
    error,
    isError,
  };
}
