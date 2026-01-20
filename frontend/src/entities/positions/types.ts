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
  departments: string[];
  isActive: boolean;
  createdAt: string;
};

export type GetPositionsRequest = {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
};

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
