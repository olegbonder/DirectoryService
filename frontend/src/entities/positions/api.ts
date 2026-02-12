import { apiClient } from "@/shared/api/axios-instance";
import { PaginationRequest, PaginationResponse } from "@/shared/api/types";
import { Position, PositionDetail } from "./types";
import { Envelope } from "@/shared/api/envelope";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { PositionsFilterState } from "@/features/positions/model/positions-filters-store";

export interface GetPositionsRequest extends PaginationRequest {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
}

export type CreatePositionRequest = {
  name: string;
  description?: string;
  departmentIds?: string[];
};

export type UpdatePositionRequest = {
  id: string;
  name: string;
  description?: string;
};

export const positionsApi = {
  getPositions: async (request: GetPositionsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Position>>
    >("/positions", {
      params: request,
    });
    return response.data.result;
  },

  getPositionDetails: async (positionId: string) => {
    const response = await apiClient.get<Envelope<PositionDetail>>(
      `/positions/${positionId}`,
    );
    return response.data.result;
  },

  createPosition: async (request: CreatePositionRequest) => {
    const response = await apiClient.post<Envelope<string>>(
      "/positions",
      request,
    );

    return response.data.result;
  },

  updatePosition: async (request: UpdatePositionRequest) => {
    const response = await apiClient.patch<Envelope<string>>(
      `/positions/${request.id}`,
      request,
    );

    return response.data.result;
  },

  deletePosition: async (positionId: string) => {
    const response = await apiClient.delete<Envelope<string>>(
      `/positions/${positionId}`,
    );

    return response.data.result;
  },
};

export const positionsQueryOptions = {
  baseKey: "positions",

  getPositionsOptions: (request: GetPositionsRequest) => {
    return queryOptions({
      queryFn: () => positionsApi.getPositions(request),
      queryKey: [positionsQueryOptions.baseKey, request],
    });
  },

  getPositionOptions: (positionId: string) => {
    return queryOptions({
      queryFn: () => positionsApi.getPositionDetails(positionId),
      queryKey: [positionsQueryOptions.baseKey, positionId],
    });
  },

  getPositionsInfinityOptions: (filter: PositionsFilterState) => {
    return infiniteQueryOptions({
      queryFn: ({ pageParam }) => {
        return positionsApi.getPositions({ ...filter, page: pageParam });
      },
      queryKey: [positionsQueryOptions.baseKey, filter],
      initialPageParam: 1,
      getNextPageParam: (response) => {
        return !response || response.page >= response.totalPages
          ? undefined
          : response.page + 1;
      },

      select: (data): PaginationResponse<Position> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? filter.pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};
