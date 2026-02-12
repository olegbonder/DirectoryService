import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { OrderDirection, PAGE_SIZE } from "@/shared/api/types";

export type DepartmentOrderColumnn = "name" | "path" | "createdAt";

export type DepartmentsFilterState = {
  name?: string;
  identifier?: string;
  parentId?: string;
  locationIds?: string[];
  isActive?: boolean;
  pageSize: number;
  orderBy?: DepartmentOrderColumnn;
  orderDirection?: OrderDirection;
};

export type DepartmentDictionaryState = {
  search?: string;
  departmentIds?: string[];
  pageSize: number;
  showOnlyParents: boolean;
};

type Actions = {
  setName: (name: DepartmentsFilterState["name"]) => void;
  setIdentifier: (identifier: DepartmentsFilterState["identifier"]) => void;
  setParentId: (parentId: DepartmentsFilterState["parentId"]) => void;
  setLocationIds: (ids: DepartmentsFilterState["locationIds"]) => void;
  setIsActive: (isActive: DepartmentsFilterState["isActive"]) => void;
  setOrderBy: (orderBy: DepartmentsFilterState["orderBy"]) => void;
  setOrderDirection: (
    orderBy: DepartmentsFilterState["orderDirection"],
  ) => void;
};

type DepartmentsFilterStore = DepartmentsFilterState & Actions;

const initialState: DepartmentsFilterState = {
  name: "",
  identifier: "",
  parentId: undefined,
  locationIds: [],
  isActive: undefined,
  pageSize: PAGE_SIZE,
  orderBy: undefined,
  orderDirection: undefined,
};

const useDepartmentsFilterStore = create<DepartmentsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setName: (name: DepartmentsFilterState["name"]) => set(() => ({ name })),
      setIdentifier: (identifier: DepartmentsFilterState["identifier"]) =>
        set(() => ({ identifier })),
      setParentId: (parentId: DepartmentsFilterState["parentId"]) =>
        set(() => ({ parentId })),
      setLocationIds: (locationIds: DepartmentsFilterState["locationIds"]) =>
        set(() => ({ locationIds })),
      setIsActive: (isActive: DepartmentsFilterState["isActive"]) =>
        set({ isActive }),
      setOrderBy: (orderBy: DepartmentsFilterState["orderBy"]) =>
        set({ orderBy }),
      setOrderDirection: (
        orderDirection: DepartmentsFilterState["orderDirection"],
      ) => set({ orderDirection }),
    }),
    {
      name: "ds-departments-filters",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useGetDepartmentsFilter = () => {
  return useDepartmentsFilterStore(
    useShallow((state) => ({
      name: state.name,
      identifier: state.identifier,
      parentId: state.parentId,
      locationIds: state.locationIds,
      isActive: state.isActive,
      pageSize: state.pageSize,
      orderBy: state.orderBy,
      orderDirection: state.orderDirection,
    })),
  );
};

export const setFilterDepartmentsName = (
  name: DepartmentsFilterState["name"],
) => {
  useDepartmentsFilterStore.getState().setName(name);
};

export const setFilterDepartmentsIdentifier = (
  identifier: DepartmentsFilterState["identifier"],
) => {
  useDepartmentsFilterStore.getState().setIdentifier(identifier);
};

export const setFilterDepartmentsParentId = (
  parentId: DepartmentsFilterState["parentId"],
) => {
  useDepartmentsFilterStore.getState().setParentId(parentId);
};

export const setFilterDepartmentsLocationIds = (
  ids: DepartmentsFilterState["locationIds"],
) => {
  useDepartmentsFilterStore.getState().setLocationIds(ids);
};

export const setFilterDepartmentsIsActive = (
  isActive: DepartmentsFilterState["isActive"],
) => {
  useDepartmentsFilterStore.getState().setIsActive(isActive);
};

export const setDepartmentsOrderBy = (
  orderBy: DepartmentsFilterState["orderBy"],
) => {
  useDepartmentsFilterStore.getState().setOrderBy(orderBy);
};

export const setDepartmentsOrderDirection = (
  orderDirection: DepartmentsFilterState["orderDirection"],
) => {
  useDepartmentsFilterStore.getState().setOrderDirection(orderDirection);
};
