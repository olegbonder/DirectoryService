export type GetDepartmentDictionaryRequest = {
  search?: string;
  page: number;
  pageSize: number;
};

export type DepartmentDictionaryState = {
  search?: string;
  departmentIds?: string[];
  pageSize: number;
};

export type AddDepartmentsToPositionRequest = {
  positionId: string;
  departmentIds: string[];
};

export type DeletePositionDepartmentRequest = {
  positionId: string;
  departmentId: string;
};
