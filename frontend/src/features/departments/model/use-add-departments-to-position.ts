import { departmentsApi } from "@/entities/departments/api";
import { positionsQueryOptions } from "@/entities/positions/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useAddDepartmentsToPosition() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.addDeparmentsToPosition,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [positionsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при добавлении подразделений в позицию");
      }
    },
    onSuccess: () => {
      toast.success("Подразделения добавлены в позицию");
    },
  });

  return {
    addDepartments: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
