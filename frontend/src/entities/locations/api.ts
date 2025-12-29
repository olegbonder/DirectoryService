import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/response";
import { GetLocationsRequest, LocationsResponse } from "./types";

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest
  ): Promise<LocationsResponse> => {
    const response = await apiClient.get<Envelope<LocationsResponse>>(
      "/locations",
      {
        params: request,
      }
    );
    return response.data.result || { locations: [], totalCount: 0 };
  },
};
