import { departmentsQueryOptions } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export function useDepartmentDictionary() {
  const query = useQuery({
    ...departmentsQueryOptions.getDictionaryOptions(),
  });
  return {
    departments: query.data?.items,
    isPending: query.isPending,
    isError: query.isError,
    error: query.error,
  };
}
