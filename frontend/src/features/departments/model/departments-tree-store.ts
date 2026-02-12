import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";
import { PAGE_SIZE } from "@/shared/api/types";

export type DepartmentsTreeState = {
  page: number;
  pageSize: number;
};

type Actions = {
  setPage: (page: DepartmentsTreeState["page"]) => void;
};

type DepartmentsTreeStore = DepartmentsTreeState & Actions;

const initialState: DepartmentsTreeState = {
  page: 1,
  pageSize: PAGE_SIZE,
};

const useDepartmentsTreeFilterStore = create<DepartmentsTreeStore>()(
  persist(
    (set) => ({
      ...initialState,
      setPage: (page: DepartmentsTreeState["page"]) => set({ page: page }),
    }),
    {
      name: "ds-departments-tree-filters",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useGetDepartmentsTreeFilter = () => {
  return useDepartmentsTreeFilterStore(
    useShallow((state) => ({
      pageSize: state.pageSize,
      page: state.page,
    })),
  );
};

export const setDepartmentsTreePage = (page: DepartmentsTreeState["page"]) => {
  useDepartmentsTreeFilterStore.getState().setPage(page);
};
