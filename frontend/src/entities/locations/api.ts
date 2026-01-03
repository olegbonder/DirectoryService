import { apiClient } from "@/shared/api/axios-instance";
import { PaginationResponse } from "@/shared/api/types";
import { GetLocationsRequest, Location } from "./types";
import { Envelope } from "@/shared/api/envelope";

export const locationsApi = {
  getLocations: async (request: GetLocationsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Location>>
    >("/locations", {
      params: request,
    });
    return response.data.result;
  },
};
