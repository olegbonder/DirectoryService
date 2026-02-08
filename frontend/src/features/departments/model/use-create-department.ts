import {
  departmentsApi,
  departmentsQueryOptions,
} from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateDepartment() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.createDepartment,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при создании подразделения");
      }
    },
    onSuccess: () => {
      toast.success("Подразделение создано");
    },
  });

  return {
    createDepartment: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
