import {
  DictionaryItemResponse,
  OrderDirection,
  PaginationRequest,
} from "@/shared/api/types";

export type Department = {
  id: string;
  parentId?: string;
  name: string;
  identifier: string;
  path: string;
  isActive: boolean;
  createdAt: Date;
};

export type DepartmentDetail = {
  id: string;
  parentId?: string;
  name: string;
  identifier: string;
  path: string;
  depth: number;
  locations: DictionaryItemResponse[];
  positions: string[];
  isActive: boolean;
  createdAt: Date;
};

export type DepartmentOrderColumnn = "name" | "path" | "createdAt";

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

export type UpdateDepartmentRequest = {
  id: string;
  name: string;
  identifier: string;
  parentId?: string;
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

export type DepartmentDictionaryState = {
  search?: string;
  departmentIds?: string[];
  pageSize: number;
  showOnlyParents: boolean;
};
