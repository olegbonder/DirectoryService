import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { DictionaryResponse } from "@/shared/api/types";
import { queryOptions } from "@tanstack/react-query";
import {
  AddDepartmentsToPositionRequest,
  DeletePositionDepartmentRequest,
} from "./types";

export const departmentsApi = {
  getDictionary: async () => {
    const response = await apiClient.get<Envelope<DictionaryResponse>>(
      "/departments/dictionary",
    );
    return response.data.result;
  },
  addDeparmentsToPosition: async (request: AddDepartmentsToPositionRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      `/positions/${request.positionId}/departments`,
      request,
    );

    return response.data.result;
  },

  deletePositionDeparment: async (request: DeletePositionDepartmentRequest) => {
    const response = await apiClient.delete<Envelope<string>>(
      `/positions/${request.positionId}/departments/${request.departmentId}`,
    );

    return response.data.result;
  },
};

export const departmentsQueryOptions = {
  baseKey: "departments",

  getDictionaryOptions: () => {
    return queryOptions({
      queryFn: () => departmentsApi.getDictionary(),
      queryKey: [departmentsQueryOptions.baseKey, "dictionary"],
    });
  },
};
