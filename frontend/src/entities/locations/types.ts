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
