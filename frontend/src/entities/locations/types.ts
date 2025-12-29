export type LocationsResponse = {
  locations: LocationDTO[];
  totalCount: number;
};

export type LocationDTO = {
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
