export type AddDepartmentsToPositionRequest = {
  positionId: string;
  departmentIds: string[];
};

export type DeletePositionDepartmentRequest = {
  positionId: string;
  departmentId: string;
};
