import { PaginationRequest } from "@/shared/api/types";

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

export interface GetLocationsRequest extends PaginationRequest {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
}

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
