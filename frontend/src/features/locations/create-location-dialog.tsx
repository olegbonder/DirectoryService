import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSeparator,
  FieldSet,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { useCreateLocation } from "./model/use-create-location";
import { GetTimezones } from "./model/location-timeZones";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";

const { timeZones } = GetTimezones();
const timeZoneRegex = /^([a-zA-Z0-9-_:]+\/[a-zA-Z0-9-_:]+)$/;

const createLocationSchema = z.object({
  name: z
    .string()
    .min(1, "Наименование обязательно")
    .min(3, "Наименование должно содержать минимум 3 символа")
    .max(120, "Наименование не должно превышать 120 символов"),
  timeZone: z.string().refine((value) => timeZoneRegex.test(value), {
    message: "Неверный формат часового пояса",
  }),
  address: z.object({
    country: z.string().min(1, "Страна обязательна"),
    city: z.string().min(1, "Город обязателен"),
    street: z.string().min(1, "Улица обязательна"),
    house: z.string().min(1, "Номер дома обязателен"),
    flatNumber: z.string().optional(),
  }),
});

type CreateLocationData = z.infer<typeof createLocationSchema>;

type CreateLocationProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export default function CreateLocationDialog({
  open,
  onOpenChange,
}: CreateLocationProps) {
  const initialData: CreateLocationData = {
    name: "",
    timeZone: "Europe/Moscow",
    address: {
      country: "",
      city: "",
      street: "",
      house: "",
      flatNumber: undefined,
    },
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateLocationData>({
    defaultValues: initialData,
    resolver: zodResolver(createLocationSchema),
  });

  const { createLocation, isPending } = useCreateLocation();

  const onSubmit = async (data: CreateLocationData) => {
    console.log(data);
    createLocation(data, {
      onSuccess: () => {
        reset(initialData);
        onOpenChange(false);
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Создать локацию</DialogTitle>
          <DialogDescription>
            Заполните форму ниже, чтобы создать новую локацию.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <FieldGroup>
            <FieldSet>
              <FieldLegend>Основные данные</FieldLegend>
              <Field data-invalid={errors.name}>
                <FieldLabel htmlFor="name" data-invalid>
                  Наименование
                </FieldLabel>
                <Input
                  id="name"
                  placeholder="Введите наименование"
                  aria-invalid={errors.name ? true : false}
                  {...register("name")}
                />
                <FieldError>{errors.name?.message}</FieldError>
              </Field>
              <Field data-invalid={errors.timeZone}>
                <FieldLabel htmlFor="timeZone">Временная зона</FieldLabel>
                <Select defaultValue="" {...register("timeZone")}>
                  <SelectTrigger
                    id="timeZone"
                    aria-invalid={errors.timeZone ? true : false}
                  >
                    <SelectValue placeholder="Выберите временную зону" />
                  </SelectTrigger>
                  <SelectContent>
                    {timeZones.map((timezone) => (
                      <SelectItem key={timezone.value} value={timezone.value}>
                        {timezone.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError>{errors.timeZone?.message}</FieldError>
              </Field>
            </FieldSet>
            <FieldSeparator />
            <FieldSet>
              <FieldLegend>Адрес</FieldLegend>
              <div className="grid grid-cols-2 gap-4">
                <Field>
                  <FieldLabel htmlFor="country">Страна</FieldLabel>
                  <Input
                    id="country"
                    type="text"
                    placeholder="Введите страну"
                    aria-invalid={errors.address?.country ? true : false}
                    {...register("address.country")}
                  />
                  <FieldError>{errors.address?.country?.message}</FieldError>
                </Field>
                <Field>
                  <FieldLabel htmlFor="city">Город</FieldLabel>
                  <Input
                    id="city"
                    type="text"
                    placeholder="Введите город"
                    aria-invalid={errors.address?.city ? true : false}
                    {...register("address.city")}
                  />
                  <FieldError>{errors.address?.city?.message}</FieldError>
                </Field>
              </div>
              <Field>
                <FieldLabel htmlFor="street">Улица</FieldLabel>
                <Input
                  id="street"
                  type="text"
                  placeholder="Введите улицу"
                  aria-invalid={errors.address?.street ? true : false}
                  {...register("address.street")}
                />
                <FieldError>{errors.address?.street?.message}</FieldError>
              </Field>
              <div className="grid grid-cols-2 gap-4">
                <Field>
                  <FieldLabel htmlFor="houseNumber">Дом</FieldLabel>
                  <Input
                    id="houseNumber"
                    type="number"
                    aria-invalid={errors.address?.house ? true : false}
                    {...register("address.house")}
                  />
                  <FieldError>{errors.address?.house?.message}</FieldError>
                </Field>
                <Field>
                  <FieldLabel htmlFor="flatNumber">Квартира</FieldLabel>
                  <Input
                    id="flatNumber"
                    type="number"
                    {...register("address.flatNumber")}
                  />
                </Field>
              </div>
            </FieldSet>
          </FieldGroup>
          <DialogFooter className="pt-6">
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Отмена
            </Button>
            <Button type="submit" disabled={isPending}>
              Создать локацию
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
