import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.createLocation,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при создании локации");
      }
    },
    onSuccess: () => {
      toast.success("Локация создана");
    },
  });

  return {
    createLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
