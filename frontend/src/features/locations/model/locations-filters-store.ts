import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { PAGE_SIZE } from "./use-locations-list";

export type LocationsFilterState = {
  departmentIds?: string[];
  search?: string;
  isActive?: boolean;
  pageSize: number;
  order: OrderBy;
};

export type OrderBy = "asc" | "desc";

type Actions = {
  setDepartmentIds: (ids: LocationsFilterState["departmentIds"]) => void;
  setSearch: (search: LocationsFilterState["search"]) => void;
  setIsActive: (isActive: LocationsFilterState["isActive"]) => void;
  setOrder: (order: LocationsFilterState["order"]) => void;
};

type LocationsFilterStore = LocationsFilterState & Actions;

const initialState: LocationsFilterState = {
  departmentIds: undefined,
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE,
  order: "asc",
};

const useLocationsFilterStore = create<LocationsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setDepartmentIds: (ids: LocationsFilterState["departmentIds"]) =>
        set(() => ({ departmentIds: ids })),
      setSearch: (input: LocationsFilterState["search"]) =>
        set(() => ({ search: input?.trim() || undefined })),
      setIsActive: (isActive: LocationsFilterState["isActive"]) =>
        set({ isActive }),
      setOrder: (order: LocationsFilterState["order"]) => set({ order }),
    }),
    {
      name: "locations-filters",
      storage: createJSONStorage(() => localStorage),
    }
  )
);

export const useGetLocationsFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      departmentIds: state.departmentIds,
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
      order: state.order,
    }))
  );
};

export const setFilterDepartmentIds = (
  ids: LocationsFilterState["departmentIds"]
) => {
  useLocationsFilterStore.getState().setDepartmentIds(ids);
};

export const setFilterSearch = (search: LocationsFilterState["search"]) => {
  useLocationsFilterStore.getState().setSearch(search);
};

export const setFilterIsActive = (
  isActive: LocationsFilterState["isActive"]
) => {
  useLocationsFilterStore.getState().setIsActive(isActive);
};

export const setOrder = (orderBy: LocationsFilterState["order"]) => {
  useLocationsFilterStore.getState().setOrder(orderBy);
};
