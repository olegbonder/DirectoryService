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

export type GetInfiniteLocationsRequest = {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  pageSize: number;
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
