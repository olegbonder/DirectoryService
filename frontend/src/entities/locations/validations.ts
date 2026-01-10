import { z } from "zod";

import { GetTimezones } from "@/entities/locations/timeZones";
const { timeZoneValues } = GetTimezones();
export const changeLocationSchema = z.object({
  name: z
    .string()
    .min(1, "Наименование обязательно")
    .min(3, "Наименование должно содержать минимум 3 символа")
    .max(120, "Наименование не должно превышать 120 символов"),
  timeZone: z.string().refine((value) => timeZoneValues.includes(value), {
    message: "Указана неправильная временная зона",
  }),
  address: z.object({
    country: z.string().min(1, "Страна обязательна"),
    city: z.string().min(1, "Город обязателен"),
    street: z.string().min(1, "Улица обязательна"),
    house: z.string().min(1, "Номер дома обязателен"),
    flat: z.string().optional(),
  }),
});

export type ChangeLocationData = z.infer<typeof changeLocationSchema>;
