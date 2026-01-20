import { positionsApi, positionsQueryOptions } from "@/entities/positions/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeletePosition() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: positionsApi.deletePosition,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [positionsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при удалении позиции");
      }
    },
    onSuccess: () => {
      toast.success("Позиция удалена");
    },
  });

  return {
    deletePosition: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
