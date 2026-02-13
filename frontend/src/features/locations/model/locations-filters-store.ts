import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { OrderDirection, PAGE_SIZE } from "@/shared/api/types";

export type LocationsFilterState = {
  departmentIds: string[];
  search: string;
  isActive?: boolean;
  pageSize: number;
  order: OrderDirection;
};

export type LocationDictionaryState = {
  search?: string;
  pageSize: number;
  locationIds?: string[];
};

type Actions = {
  setDepartmentIds: (ids: LocationsFilterState["departmentIds"]) => void;
  setSearch: (search: LocationsFilterState["search"]) => void;
  setIsActive: (isActive: LocationsFilterState["isActive"]) => void;
  setOrder: (order: LocationsFilterState["order"]) => void;
};

type LocationsFilterStore = LocationsFilterState & Actions;

const initialState: LocationsFilterState = {
  departmentIds: [],
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
        set(() => ({ search: input.trim() || "" })),
      setIsActive: (isActive: LocationsFilterState["isActive"]) =>
        set({ isActive }),
      setOrder: (order: LocationsFilterState["order"]) => set({ order }),
    }),
    {
      name: "ds-locations-filters",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useGetLocationsFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      departmentIds: state.departmentIds,
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
      order: state.order,
    })),
  );
};

export const setFilterLocationsDepartmentIds = (
  ids: LocationsFilterState["departmentIds"],
) => {
  useLocationsFilterStore.getState().setDepartmentIds(ids);
};

export const setFilterLocationsSearch = (
  search: LocationsFilterState["search"],
) => {
  useLocationsFilterStore.getState().setSearch(search);
};

export const setFilterLocationsIsActive = (
  isActive: LocationsFilterState["isActive"],
) => {
  useLocationsFilterStore.getState().setIsActive(isActive);
};

export const setLocationsOrder = (orderBy: LocationsFilterState["order"]) => {
  useLocationsFilterStore.getState().setOrder(orderBy);
};
