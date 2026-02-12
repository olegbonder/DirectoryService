import { DictionaryItemResponse } from "@/shared/api/types";

export type Department = {
  id: string;
  parentId?: string;
  name: string;
  identifier: string;
  path: string;
  isActive: boolean;
  createdAt: Date;
};

export type DepartmentDetail = {
  id: string;
  parentId?: string;
  name: string;
  identifier: string;
  path: string;
  depth: number;
  locations: DictionaryItemResponse[];
  positions: string[];
  isActive: boolean;
  createdAt: Date;
};

export type ChildDepartment = {
  id: string;
  parentId?: string;
  name: string;
  identifier: string;
  path: string;
  depth: number;
  isActive: boolean;
  createdAt: Date;
  hasMoreChildren: boolean;
};

export type RootDepartment = {
  id: string;
  parentId?: string;
  name: string;
  identifier: string;
  path: string;
  depth: number;
  isActive: boolean;
  createdAt: Date;
  children: ChildDepartment[];
  hasMoreChildren: boolean;
};
