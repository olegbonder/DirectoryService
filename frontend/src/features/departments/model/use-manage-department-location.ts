import {
  departmentsApi,
  departmentsQueryOptions,
} from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useManageDepartmentLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.updateDepartmentLocations,
    onSettled: (_, __, variables) => {
      // Invalidate the specific department query to refresh the data
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey, variables.departmentId],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при обновлении локаций у подразделения");
      }
    },
    onSuccess: () => {
      toast.success("Локации подразделения обновлены");
    },
  });

  return {
    updateLocations: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
