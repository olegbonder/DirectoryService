import {
  FieldGroup,
  FieldSet,
  FieldLegend,
  Field,
  FieldLabel,
  FieldError,
  FieldSeparator,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { GetTimezones } from "@/entities/locations/timeZones";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  Control,
  Controller,
  FieldErrors,
  UseFormRegister,
} from "react-hook-form";
import { ChangeLocationData } from "@/entities/locations/validations";

const { timeZones } = GetTimezones();

type ChangeLocationFormProps = {
  control: Control<ChangeLocationData>;
  register: UseFormRegister<ChangeLocationData>;
  errors: FieldErrors<ChangeLocationData>;
};

export default function ChangeLocationForm({
  control,
  register,
  errors,
}: ChangeLocationFormProps) {
  return (
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
          <Controller
            name="timeZone"
            control={control}
            render={({ field }) => (
              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger aria-invalid={errors.timeZone ? true : false}>
                  <SelectValue
                    id="timeZone"
                    placeholder="Выберите временную зону"
                  />
                </SelectTrigger>
                <SelectContent>
                  {timeZones.map((timezone) => (
                    <SelectItem key={timezone.value} value={timezone.value}>
                      {timezone.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          />
          {/*<select {...register("timeZone")}>
            {timeZones.map((timezone) => (
              <option key={timezone.value} value={timezone.value}>
                {timezone.label}
              </option>
            ))}
          </select>*/}
          {/*<Select {...register("timeZone")}>
            <SelectTrigger aria-invalid={errors.timeZone ? true : false}>
              <SelectValue
                id="timeZone"
                placeholder="Выберите временную зону"
              />
            </SelectTrigger>
            <SelectContent>
              {timeZones.map((timezone) => (
                <SelectItem key={timezone.value} value={timezone.value}>
                  {timezone.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>*/}
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
              id="house"
              type="number"
              aria-invalid={errors.address?.house ? true : false}
              {...register("address.house")}
            />
            <FieldError>{errors.address?.house?.message}</FieldError>
          </Field>
          <Field>
            <FieldLabel htmlFor="flat">Квартира</FieldLabel>
            <Input id="flat" type="number" {...register("address.flat")} />
          </Field>
        </div>
      </FieldSet>
    </FieldGroup>
  );
}
