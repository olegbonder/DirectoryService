import { z } from "zod";

export const createPositionSchema = z.object({
  name: z
    .string()
    .min(1, "Наименование обязательно")
    .min(3, "Наименование должно содержать минимум 3 символа")
    .max(100, "Наименование не должно превышать 100 символов"),
  description: z
    .string()
    .max(1000, "Описание не должно превышать 1000 символов")
    .optional(),
  departmentIds: z
    .array(z.guid("Неправильный формат идентификатора подразделения"))
    .min(1, "Выберите хотя бы одно подразделение"),
});

export type CreatePositionData = z.infer<typeof createPositionSchema>;
