import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import { useCreateLocation } from "./model/use-create-location";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { CreateLocationRequest } from "@/entities/locations/types";
import z from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

const createLocationSchema = z.object({
  name: z
    .string()
    .min(1, "Имя обязательно")
    .min(3, "Минимум 3 символа")
    .max(200, "Не должно превышать 100 символов"),
  officeNumber: z.string().min(1, "Укажите номер офиса/помещения"),
  buildingNumber: z.string().min(1, "Укажите номер здания"),
  street: z.string().min(1, "Укажите улицу"),
  city: z.string().min(1, "Укажите город"),
  country: z.string().min(1, "Укажите страну"),
  timezone: z.string().min(1, "Выберите часовой пояс"),
  stateOrProvince: z.string().optional(),
  postalCode: z.string().optional(),
});

type CreateLocationFormValues = z.infer<typeof createLocationSchema>;

export function CreateLocationDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const defaultData: CreateLocationFormValues = {
    name: "",
    officeNumber: "",
    buildingNumber: "",
    street: "",
    city: "",
    stateOrProvince: undefined,
    country: "",
    postalCode: undefined,
    timezone: "Europe/Moscow",
  };

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateLocationFormValues>({
    defaultValues: defaultData,
    resolver: zodResolver(createLocationSchema),
  });

  const { createLocation, isPending, isError, error } = useCreateLocation();

  const onSubmit = ({
    name,
    timezone,
    ...addressFields
  }: CreateLocationFormValues) => {
    const request: CreateLocationRequest = {
      name,
      timezone,
      address: addressFields,
    };

    createLocation(request, {
      onSuccess: () => {
        onOpenChange(false);
        reset();
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-187.5">
        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogTitle>Создание локации</DialogTitle>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 py-4">
            {/* Левая колонка */}
            <div className="flex flex-col gap-4">
              <FieldGroup>
                <Field data-invalid={!!errors.name}>
                  <FieldLabel htmlFor="name">Название</FieldLabel>
                  <Input
                    id="name"
                    placeholder="Название"
                    {...register("name")}
                  />
                  <FieldError>{errors.name?.message}</FieldError>
                </Field>

                <Field data-invalid={!!errors.timezone}>
                  <FieldLabel htmlFor="timezone">Часовой пояс</FieldLabel>
                  <Input
                    id="timezone"
                    placeholder="Europe/Moscow"
                    {...register("timezone")}
                  />
                  <FieldError>{errors.timezone?.message}</FieldError>
                </Field>
              </FieldGroup>
            </div>

            {/* Правая колонка */}
            <div className="flex flex-col gap-4">
              <FieldGroup>
                <Field data-invalid={!!errors.officeNumber}>
                  <FieldLabel htmlFor="officeNumber">Номер офиса</FieldLabel>
                  <Input
                    id="officeNumber"
                    placeholder="A12"
                    {...register("officeNumber")}
                  />
                  <FieldError>{errors.officeNumber?.message}</FieldError>
                </Field>

                <Field data-invalid={!!errors.buildingNumber}>
                  <FieldLabel htmlFor="buildingNumber">Номер дома</FieldLabel>
                  <Input
                    id="buildingNumber"
                    placeholder="45"
                    {...register("buildingNumber")}
                  />
                  <FieldError>{errors.buildingNumber?.message}</FieldError>
                </Field>

                <Field data-invalid={!!errors.street}>
                  <FieldLabel htmlFor="street">Улица</FieldLabel>
                  <Input
                    id="street"
                    placeholder="Тверская"
                    {...register("street")}
                  />
                  <FieldError>{errors.street?.message}</FieldError>
                </Field>

                <Field data-invalid={!!errors.city}>
                  <FieldLabel htmlFor="city">Город</FieldLabel>
                  <Input id="city" placeholder="Москва" {...register("city")} />
                  <FieldError>{errors.city?.message}</FieldError>
                </Field>

                <Field data-invalid={!!errors.stateOrProvince}>
                  <FieldLabel htmlFor="stateOrProvince">Регион</FieldLabel>
                  <Input
                    id="stateOrProvince"
                    placeholder="Москва"
                    {...register("stateOrProvince")}
                  />
                  <FieldError>{errors.stateOrProvince?.message}</FieldError>
                </Field>

                <Field data-invalid={!!errors.country}>
                  <FieldLabel htmlFor="country">Страна</FieldLabel>
                  <Input
                    id="country"
                    placeholder="РФ"
                    {...register("country")}
                  />
                  <FieldError>{errors.country?.message}</FieldError>
                </Field>

                <Field data-invalid={!!errors.postalCode}>
                  <FieldLabel htmlFor="postalCode">Индекс</FieldLabel>
                  <Input
                    id="postalCode"
                    placeholder="11144"
                    {...register("postalCode")}
                  />
                  <FieldError>{errors.postalCode?.message}</FieldError>
                </Field>
              </FieldGroup>
            </div>
          </div>

          <DialogFooter className="flex flex-col sm:flex-row gap-4 items-center justify-between w-full">
            <Field orientation="horizontal">
              <Button
                type="button"
                variant="outline"
                onClick={() => reset()}
                disabled={isPending}
              >
                Сброс
              </Button>
              <Button type="submit" disabled={isPending}>
                Создать
                {isPending && <Spinner className="ml-2" />}
              </Button>
            </Field>

            {isError && (
              <div className="text-destructive text-sm font-medium">
                {error
                  ? error.firstMessage
                  : "Неизвестная ошибка или сбой сети"}
              </div>
            )}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
