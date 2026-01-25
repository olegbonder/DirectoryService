import { departmentsApi } from "@/entities/departments/api";
import { positionsQueryOptions } from "@/entities/positions/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeletePositionDepartment() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: departmentsApi.deletePositionDeparment,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [positionsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при удалении подразделения у позиции");
      }
    },
    onSuccess: () => {
      toast.success("Подразделение удалено");
    },
  });

  return {
    deleteDeparment: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
