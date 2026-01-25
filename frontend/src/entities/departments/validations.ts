import { z } from "zod";

export const addDepartmentsToPositionSchema = z.object({
  departmentIds: z
    .array(z.guid("Неправильный формат идентификатора подразделения"))
    .min(1, "Выберите хотя бы одно подразделение"),
});

export type AddDepartmentsToPositionData = z.infer<
  typeof addDepartmentsToPositionSchema
>;
