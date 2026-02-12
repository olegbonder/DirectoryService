import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import {
  DictionaryItemResponse,
  OrderDirection,
  PAGE_SIZE,
  PaginationRequest,
  PaginationResponse,
  PREFETCH,
} from "@/shared/api/types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import {
  Department,
  DepartmentDetail,
  ChildDepartment,
  RootDepartment,
} from "./types";
import {
  DepartmentDictionaryState,
  DepartmentOrderColumnn,
  DepartmentsFilterState,
} from "@/features/departments/model/departments-filters-store";

export interface GetDepartmentsRequest extends PaginationRequest {
  name?: string;
  identifier?: string;
  parentId?: string;
  locationIds?: string[];
  isActive?: boolean;
  orderBy?: DepartmentOrderColumnn;
  orderDirection?: OrderDirection;
}

export interface GetRootDepartmentsRequest extends PaginationRequest {
  prefetch?: number;
}

export interface GetChildDepartmentsRequest extends PaginationRequest {
  parentId: string;
}

export type CreateDepartmentRequest = {
  name: string;
  identifier: string;
  parentId?: string;
  locationIds: string[];
};

export type UpdateAndMoveDepartmentRequest = {
  id: string;
  name: string;
  identifier: string;
  parentId?: string;
};

export type UpdateDepartmentRequest = {
  id: string;
  name: string;
  identifier: string;
};

export type MoveDepartmentRequest = {
  id: string;
  parentId?: string;
};

export type UpdateDepartmentLocationsRequest = {
  departmentId: string;
  locationIds: string[];
};

export interface GetDepartmentDictionaryRequest extends PaginationRequest {
  search?: string;
  showOnlyParents: boolean;
}

export type AddDepartmentsToPositionRequest = {
  positionId: string;
  departmentIds: string[];
};

export type DeletePositionDepartmentRequest = {
  positionId: string;
  departmentId: string;
};

export const departmentsApi = {
  getDepartmentDictionary: async (request: GetDepartmentDictionaryRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<DictionaryItemResponse>>
    >("/departments/dictionary", {
      params: request,
    });
    return response.data.result;
  },

  getRootDepartments: async (request: GetRootDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<RootDepartment>>
    >("/departments/roots", {
      params: request,
    });
    return response.data.result;
  },

  getChildDepartments: async (request: GetChildDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<ChildDepartment>>
    >(`/departments/${request.parentId}/children`, {
      params: request,
    });
    return response.data.result;
  },

  getDepartments: async (request: GetDepartmentsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Department>>
    >("/departments", {
      params: request,
    });
    return response.data.result;
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

  moveDepartment: async (request: MoveDepartmentRequest) => {
    const response = await apiClient.put<Envelope<string>>(
      `/departments/${request.id}/parent`,
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

  updateDepartmentLocations: async (
    request: UpdateDepartmentLocationsRequest,
  ) => {
    const response = await apiClient.put<Envelope<string>>(
      `/departments/${request.departmentId}/locations`,
      request,
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
        return departmentsApi.getDepartmentDictionary({
          ...filter,
          page: pageParam,
        });
      },
      queryKey: ["departments-dictionary", filter],
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

  getDepartmentRootsInfinityOptions: () => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return departmentsApi.getRootDepartments({
          pageSize: PAGE_SIZE,
          prefetch: PREFETCH,
          page: pageParam,
        });
      },
      queryKey: [departmentsQueryOptions.baseKey, "roots"],
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },

      select: (data): PaginationResponse<RootDepartment> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? PAGE_SIZE,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },

  getDepartmentChildrenInfinityOptions: (parentId: string) => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return departmentsApi.getChildDepartments({
          pageSize: PAGE_SIZE,
          parentId,
          page: pageParam,
        });
      },
      queryKey: [departmentsQueryOptions.baseKey, parentId, "children"],
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },

      select: (data): PaginationResponse<ChildDepartment> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? PAGE_SIZE,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};
