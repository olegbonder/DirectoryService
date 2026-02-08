import { DictionaryItemResponse, PaginationRequest } from "@/shared/api/types";

export type Position = {
  id: string;
  name: string;
  description?: string;
  departmentsCount: number;
  isActive: boolean;
};

export type PositionDetail = {
  id: string;
  name: string;
  description?: string;
  departments: DictionaryItemResponse[];
  isActive: boolean;
  createdAt: string;
};

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
