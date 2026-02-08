import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { DictionaryItemResponse, PaginationResponse } from "@/shared/api/types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import {
  AddDepartmentsToPositionRequest,
  CreateDepartmentRequest,
  DeletePositionDepartmentRequest,
  Department,
  DepartmentDetail,
  DepartmentDictionaryState,
  GetDepartmentDictionaryRequest,
  GetDepartmentsRequest,
  GetRootDepartmentsRequest,
  GetChildDepartmentsRequest,
  UpdateDepartmentRequest,
} from "./types";
import { DepartmentsFilterState } from "@/features/departments/model/departments-filters-store";

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

  getRootDepartments: async (request: GetRootDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Department>>
    >("/departments/roots", {
      params: request,
      paramsSerializer: {
        indexes: null,
      },
    });
    return response.data.result;
  },

  getChildDepartments: async (request: GetChildDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Department>>
    >(`/departments/${request.parentId}/children`, {
      params: request,
    });
    return response.data.result;
  },

  getDepartments: async (request: GetDepartmentsRequest) => {
    // Создаем копию параметров
    const params = { ...request };

    // Если есть параметр сортировки, преобразуем его в нужный формат
    if (params.orderColumnn) {
      const transformedParams = { ...params };
      transformedParams["OrderColumn.Field"] =
        params.orderColumnn.field.charAt(0).toUpperCase() +
        params.orderColumnn.field.slice(1);
      transformedParams["OrderColumn.Direction"] =
        params.orderColumnn.direction.charAt(0).toUpperCase() +
        params.orderColumnn.direction.slice(1);
      delete transformedParams.orderColumnn;

      const response = await apiClient.get<
        Envelope<PaginationResponse<Department>>
      >("/departments", {
        params: transformedParams,
        paramsSerializer: {
          indexes: null,
        },
      });
      return response.data.result;
    } else {
      const response = await apiClient.get<
        Envelope<PaginationResponse<Department>>
      >("/departments", {
        params: request,
        paramsSerializer: {
          indexes: null,
        },
      });
      return response.data.result;
    }
  },

  getDepartmentDetails: async (departmentId: string) => {
    const response = await apiClient.get<Envelope<DepartmentDetail>>(
      `/departments/${departmentId}`,
    );
    return response.data.result;
  },

  createDepartment: async (request: CreateDepartmentRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      "/departments",
      request,
    );

    return response.data.result;
  },

  updateDepartment: async (request: UpdateDepartmentRequest) => {
    const response = await apiClient.patch<Envelope<string>>(
      `/departments/${request.id}`,
      request,
    );

    return response.data.result;
  },

  deleteDepartment: async (departmentId: string) => {
    const response = await apiClient.delete<Envelope<string>>(
      `/departments/${departmentId}`,
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

  getDepartmentOptions: (departmentId: string) => {
    return queryOptions({
      queryFn: () => departmentsApi.getDepartmentDetails(departmentId),
      queryKey: [departmentsQueryOptions.baseKey, departmentId],
    });
  },

  getDepartmentsInfinityOptions: (filter: DepartmentsFilterState) => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return departmentsApi.getDepartments({ ...filter, page: pageParam });
      },
      queryKey: [departmentsQueryOptions.baseKey, filter],
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },

      select: (data): PaginationResponse<Department> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? filter.pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};
