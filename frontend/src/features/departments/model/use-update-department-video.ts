import {
  departmentsApi,
  departmentsQueryOptions,
} from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

type UpdateDepartmentVideoParams = {
  departmentId: string;
  videoId?: string;
};

export function useUpdateDepartmentVideo() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async ({
      departmentId,
      videoId,
    }: UpdateDepartmentVideoParams) => {
      await departmentsApi.updateDepartmentVideo({ departmentId, videoId });
    },
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при обновлении видео урока");
    },
    onSuccess: () => {
      toast.success("Видео успешно прикреплено к уроку");
    },
  });

  return {
    updateDepartmentVideo: mutation.mutate,
    isPending: mutation.isPending,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isError: mutation.isError,
  };
}
