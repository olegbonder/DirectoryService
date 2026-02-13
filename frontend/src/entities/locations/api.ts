import { apiClient } from "@/shared/api/axios-instance";
import {
  PaginationResponse,
  DictionaryItemResponse,
  PaginationRequest,
} from "@/shared/api/types";
import { Location, Address } from "./types";
import { Envelope } from "@/shared/api/envelope";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import {
  LocationDictionaryState,
  LocationsFilterState,
} from "@/features/locations/model/locations-filters-store";

export interface GetLocationsRequest extends PaginationRequest {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
}

export interface GetLocationDictionaryRequest extends PaginationRequest {
  search?: string;
}

export type CreateLocationRequest = {
  name: string;
  address: Address;
  timeZone: string;
};

export type UpdateLocationRequest = {
  id: string;
  name: string;
  address: Address;
  timeZone: string;
};

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Location>>
    >("/locations", {
      params: request,
    });
    return response.data.result;
  },

  getLocationDictionary: async (request: GetLocationDictionaryRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<DictionaryItemResponse>>
    >("/locations/dictionary", {
      params: request,
    });
    return response.data.result;
  },

  createLocation: async (request: CreateLocationRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      "/locations",
      request,
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
      `/locations/${locationId}`,
    );

    return response.data.result;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",

  getLocationsOptions: (request: GetLocationsRequest) => {
    return queryOptions({
      queryFn: () => locationsApi.getLocations(request),
      queryKey: [locationsQueryOptions.baseKey, request],
    });
  },

  getLocationDictionaryInfinityOptions: (filter: LocationDictionaryState) => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return locationsApi.getLocationDictionary({
          ...filter,
          page: pageParam,
        });
      },
      queryKey: [locationsQueryOptions.baseKey, filter],
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },

      select: (data): PaginationResponse<DictionaryItemResponse> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? filter.pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },

  getLocationsInfinityOptions: (filter: LocationsFilterState) => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return locationsApi.getLocations({
          ...filter,
          page: pageParam,
        });
      },
      queryKey: [locationsQueryOptions.baseKey, filter],
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response ||
          response.page >= response.totalPages ||
          filter.departmentIds?.length > 0
          ? undefined
          : response.page + 1;
      },

      select: (data): PaginationResponse<Location> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? filter.pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};
