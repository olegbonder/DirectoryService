import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { PAGE_SIZE } from "./use-positions-list";

export type PositionsFilterState = {
  departmentIds: string[];
  search: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
};

type Actions = {
  setDepartmentIds: (ids: PositionsFilterState["departmentIds"]) => void;
  setSearch: (search: PositionsFilterState["search"]) => void;
  setIsActive: (isActive: PositionsFilterState["isActive"]) => void;
  setPage: (page: PositionsFilterState["page"]) => void;
};

type PositionsFilterStore = PositionsFilterState & Actions;

const initialState: PositionsFilterState = {
  departmentIds: [],
  search: "",
  isActive: undefined,
  page: 1,
  pageSize: PAGE_SIZE,
};

const usePositionsFilterStore = create<PositionsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setDepartmentIds: (ids: PositionsFilterState["departmentIds"]) =>
        set(() => ({ departmentIds: ids })),
      setSearch: (input: PositionsFilterState["search"]) =>
        set(() => ({ search: input.trim() || "" })),
      setIsActive: (isActive: PositionsFilterState["isActive"]) =>
        set({ isActive }),
      setPage: (page: PositionsFilterState["page"]) => set({ page: page }),
    }),
    {
      name: "ds-positions-filters",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useGetPositionsFilter = () => {
  return usePositionsFilterStore(
    useShallow((state) => ({
      departmentIds: state.departmentIds,
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
      page: state.page,
    })),
  );
};

export const setFilterPositionsDepartmentIds = (
  ids: PositionsFilterState["departmentIds"],
) => {
  usePositionsFilterStore.getState().setDepartmentIds(ids);
};

export const setFilterPositionsSearch = (
  search: PositionsFilterState["search"],
) => {
  usePositionsFilterStore.getState().setSearch(search);
};

export const setFilterPositionsIsActive = (
  isActive: PositionsFilterState["isActive"],
) => {
  usePositionsFilterStore.getState().setIsActive(isActive);
};

export const setPositionsPage = (page: PositionsFilterState["page"]) => {
  usePositionsFilterStore.getState().setPage(page);
};
