import { z } from "zod";

const positionNameValidation = z
  .string()
  .min(1, "Наименование обязательно")
  .min(3, "Наименование должно содержать минимум 3 символа")
  .max(100, "Наименование не должно превышать 100 символов");

const positionDescriptionValidation = z
  .string()
  .max(1000, "Описание не должно превышать 1000 символов")
  .optional();

export const createPositionSchema = z.object({
  name: positionNameValidation,
  description: positionDescriptionValidation,
  departmentIds: z
    .array(z.guid("Неправильный формат идентификатора подразделения"))
    .min(1, "Выберите хотя бы одно подразделение"),
});

export type CreatePositionData = z.infer<typeof createPositionSchema>;

export const updatePositionSchema = z.object({
  name: positionNameValidation,
  description: positionDescriptionValidation,
});

export type UpdatePositionFormData = z.infer<typeof updatePositionSchema>;
