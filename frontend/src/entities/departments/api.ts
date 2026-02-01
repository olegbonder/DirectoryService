import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { DictionaryItemResponse, PaginationResponse } from "@/shared/api/types";
import { infiniteQueryOptions } from "@tanstack/react-query";
import {
  AddDepartmentsToPositionRequest,
  DeletePositionDepartmentRequest,
  DepartmentDictionaryState,
  GetDepartmentDictionaryRequest,
} from "./types";

export const departmentsApi = {
  getDictionary: async (request: GetDepartmentDictionaryRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<DictionaryItemResponse>>
    >("/departments/dictionary", {
      params: request,
      paramsSerializer: {
        indexes: null,
      },
    });
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

  getDepartmentDictionaryInfinityOptions: (
    filter: DepartmentDictionaryState,
  ) => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return departmentsApi.getDictionary({
          ...filter,
          page: pageParam,
        });
      },
      queryKey: [departmentsQueryOptions.baseKey, filter],
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
};
