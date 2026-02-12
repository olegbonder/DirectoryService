import { z } from "zod";

const departmentNameValidation = z
  .string()
  .min(1, "Наименование обязательно")
  .min(3, "Наименование должно содержать минимум 3 символа")
  .max(150, "Наименование не должно превышать 150 символов");

const departmentIdentifierValidation = z
  .string()
  .min(1, "Идентификатор обязателен")
  .min(3, "Идентификатор должен содержать минимум 3 символа")
  .max(150, "Идентификатор не должен превышать 150 символов")
  .regex(/^[a-z]+$/, "Идентификатор должен состоять только из латинских букв");

const departmentParentIdValidation = z
  .guid("Неправильный формат идентификатора подразделения")
  .optional();

const departmentLocationIdsValidation = z
  .array(z.guid("Неправильный формат идентификатора локации"))
  .min(1, "Выберите хотя бы одну локацию");

export const createDepartmentSchema = z.object({
  name: departmentNameValidation,
  identifier: departmentIdentifierValidation,
  parentId: departmentParentIdValidation,
  locationIds: departmentLocationIdsValidation,
});

export type CreateDepartmentData = z.infer<typeof createDepartmentSchema>;

export const addDepartmentsToPositionSchema = z.object({
  departmentIds: z
    .array(z.guid("Неправильный формат идентификатора подразделения"))
    .min(1, "Выберите хотя бы одно подразделение"),
});

export type AddDepartmentsToPositionData = z.infer<
  typeof addDepartmentsToPositionSchema
>;

export const updateDepartmentSchema = z.object({
  name: departmentNameValidation,
  identifier: departmentIdentifierValidation,
  parentId: departmentParentIdValidation,
});

export type UpdateDepartmentData = z.infer<typeof updateDepartmentSchema>;

export const manageDepartmentLocationsSchema = z.object({
  locationIds: departmentLocationIdsValidation,
});

export type ManageDepartmentLocationsData = z.infer<
  typeof manageDepartmentLocationsSchema
>;
