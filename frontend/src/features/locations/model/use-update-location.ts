import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.updateLocation,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при обновлении локации");
      }
    },
    onSuccess: () => {
      toast.success("Локация обновлена");
    },
  });

  return {
    updateLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
