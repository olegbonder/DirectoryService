export type Address = {
  country: string;
  city: string;
  street: string;
  house: string;
  flat?: string;
};

export interface Location extends Address {
  id: string;
  name: string;
  timeZone: string;
  isActive: boolean;
  createdAt: string;
}

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

export type CreateLocationRequest = {
  name: string;
  address: Address;
  timeZone: string;
};

export type UpdateLocationRequest = {
  id: string;
  name: string;
  address: Address;
  timeZone: string;
};
