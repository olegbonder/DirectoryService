import { apiClient } from "@/shared/api/axios-instance";
import { PaginationResponse } from "@/shared/api/types";
import {
  CreateLocationRequest,
  GetLocationsRequest,
  GetInfiniteLocationsRequest,
  Location,
  UpdateLocationRequest,
} from "./types";
import { Envelope } from "@/shared/api/envelope";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Location>>
    >("/locations", {
      params: request,
    });
    return response.data.result;
  },

  createLocation: async (request: CreateLocationRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      "/locations",
      request
    );

    return response.data.result;
  },

  updateLocation: async ({
    id,
    name,
    address,
    timeZone,
  }: UpdateLocationRequest) => {
    const response = await apiClient.put<Envelope<string>>(`/locations/${id}`, {
      name,
      address,
      timeZone,
    });

    return response.data.result;
  },

  deleteLocation: async (locationId: string) => {
    const response = await apiClient.delete<Envelope<string>>(
      `/locations/${locationId}`
    );

    return response.data.result;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",

  getLocationsOptions: (request: GetLocationsRequest) => {
    return queryOptions({
      queryFn: () => locationsApi.getLocations(request),
      queryKey: [
        locationsQueryOptions.baseKey,
        request.page,
        request.departmentIds,
        request.search,
        request.isActive,
      ],
    });
  },

  getLocationsInfinityOptions: (request: GetInfiniteLocationsRequest) => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return locationsApi.getLocations({ ...request, page: pageParam });
      },
      queryKey: [
        locationsQueryOptions.baseKey,
        request.departmentIds,
        request.search,
        request.isActive,
      ],
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },

      select: (data): PaginationResponse<Location> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? request.pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};
