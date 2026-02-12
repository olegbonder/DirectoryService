import {
  departmentsApi,
  departmentsQueryOptions,
  UpdateAndMoveDepartmentRequest,
} from "@/entities/departments/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateAndMoveDepartment() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async ({
      id,
      name,
      identifier,
      parentId,
    }: UpdateAndMoveDepartmentRequest) => {
      // Сначала обновляем данные отдела
      const departmentId = await departmentsApi.updateDepartment({
        id,
        name,
        identifier,
      });

      // Затем перемещаем отдел, если указана новая позиция
      if (departmentId) {
        await departmentsApi.moveDepartment({ id: departmentId, parentId });
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [departmentsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
      } else {
        toast.error("Ошибка при обновлении подразделения");
      }
    },
    onSuccess: () => {
      toast.success("Подразделение обновлено");
    },
  });

  return {
    updateDepartment: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
