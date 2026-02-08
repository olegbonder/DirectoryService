import { departmentsQueryOptions } from "@/entities/departments/api";
import { useQuery } from "@tanstack/react-query";

export default function useDepartment(departmentId: string) {
  const { data, isPending, error, isError, refetch } = useQuery({
    ...departmentsQueryOptions.getDepartmentOptions(departmentId),
  });

  return {
    department: data,
    isPending,
    error,
    isError,
    refetch,
  };
}
