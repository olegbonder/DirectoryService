export type Location = {
  id: string;
  name: string;
  country: string;
  city: string;
  street: string;
  houseNumber: string;
  flatNumber?: string;
  timezone: string;
  isActive: boolean;
  createdAt: string;
};

export type GetLocationsRequest = {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
};

export type LocationFilterProps = {
  departmentIds?: string[];
  setDepartmentIds: (ids: string[] | undefined) => void;
  search?: string;
  setSearch: (search: string | undefined) => void;
  isActive?: boolean;
  setIsActive: (isActive: boolean) => void;
};
