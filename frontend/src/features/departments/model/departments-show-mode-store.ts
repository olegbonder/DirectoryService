import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";

export type DepartmentsViewMode = "tree" | "list";

export type DepartmentsViewModeState = {
  viewMode: DepartmentsViewMode;
};

type Actions = {
  setViewMode: (viewMode: DepartmentsViewModeState["viewMode"]) => void;
};

type DepartmentsViewModeStore = DepartmentsViewModeState & Actions;

const initialState: DepartmentsViewModeState = {
  viewMode: "tree",
};

const useDepartmentsViewModeStore = create<DepartmentsViewModeStore>()(
  persist(
    (set) => ({
      ...initialState,
      setViewMode: (viewMode: DepartmentsViewModeState["viewMode"]) =>
        set({ viewMode }),
    }),
    {
      name: "ds-departments-view-mode",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useDepartmentsViewMode = () => {
  return useDepartmentsViewModeStore(
    useShallow((state) => ({
      viewMode: state.viewMode,
    })),
  );
};

export const setDepartmentsViewMode = (
  viewMode: DepartmentsViewModeStore["viewMode"],
) => {
  useDepartmentsViewModeStore.getState().setViewMode(viewMode);
};
