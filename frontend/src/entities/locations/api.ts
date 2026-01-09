import { apiClient } from "@/shared/api/axios-instance";
import { PaginationResponse } from "@/shared/api/types";
import {
  CreateLocationRequest,
  GetLocationsRequest,
  Location,
  UpdateLocationRequest,
} from "./types";
import { Envelope } from "@/shared/api/envelope";
import { queryOptions } from "@tanstack/react-query";

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
};
