import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type GlobalFilterState = {
  search: string;
};

type Actions = {
  setSearch: (search: GlobalFilterState["search"]) => void;
};

type GlobalFilterStore = GlobalFilterState & Actions;

const initialState: GlobalFilterState = {
  search: "",
};

const useGlobalSearchStore = create<GlobalFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setSearch: (input: GlobalFilterState["search"]) =>
        set(() => ({ search: input.trim() || "" })),
    }),
    {
      name: "ds-global-search",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useGetGlobalFilter = () => {
  return useGlobalSearchStore((state) => state.search);
};

export const setGlobalFilterSearch = (search: GlobalFilterState["search"]) => {
  useGlobalSearchStore.getState().setSearch(search);
};
