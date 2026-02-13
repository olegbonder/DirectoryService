import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";

export type DepartmentsTreeState = {
  expandedNodes: string[];
};

type Actions = {
  setExpandedNodes: (
    expandedNodes: DepartmentsTreeState["expandedNodes"],
  ) => void;
};

type DepartmentsTreeStore = DepartmentsTreeState & Actions;

const initialState: DepartmentsTreeState = {
  expandedNodes: [],
};

const useDepartmentsTreeStore = create<DepartmentsTreeStore>()(
  persist(
    (set) => ({
      ...initialState,
      setExpandedNodes: (
        expandedNodes: DepartmentsTreeState["expandedNodes"],
      ) => set({ expandedNodes }),
    }),
    {
      name: "ds-departments-tree-nodes",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useDepartmentsExpandedNodes = () => {
  return useDepartmentsTreeStore(
    useShallow((state) => ({
      expandedNodes: state.expandedNodes,
    })),
  );
};

export const setDepartmentsTreeExpandedNodes = (
  expandedNodes: DepartmentsTreeState["expandedNodes"],
) => {
  useDepartmentsTreeStore.getState().setExpandedNodes(expandedNodes);
};
