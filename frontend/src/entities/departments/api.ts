import { apiClient } from "@/shared/api/axios-instance";

import { Envelope } from "@/shared/api/envelope";
import { DictionaryResponse } from "@/shared/api/types";
import { queryOptions } from "@tanstack/react-query";

export const departmentsApi = {
  getDictionary: async () => {
    const response = await apiClient.get<Envelope<DictionaryResponse>>(
      "/departments/dictionary",
    );
    return response.data.result;
  },
};

export const departmentsQueryOptions = {
  baseKey: "departments",

  getDictionaryOptions: () => {
    return queryOptions({
      queryFn: () => departmentsApi.getDictionary(),
      queryKey: [departmentsQueryOptions.baseKey, "dictionary"],
    });
  },
};
