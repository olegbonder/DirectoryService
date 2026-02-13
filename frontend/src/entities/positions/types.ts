import { DictionaryItemResponse } from "@/shared/api/types";

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
  departments: DictionaryItemResponse[];
  isActive: boolean;
  createdAt: string;
};
