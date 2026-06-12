import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/ui/dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import { useCreateLocation } from "./model/use-create-location";
import {
  Field,
  FieldLabel,
  FieldError,
  FieldGroup,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { CreateLocationRequest } from "@/entities/locations/types";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CreateLocationFormValues, createLocationSchema } from "./model/types";
import { AlertCircle } from "lucide-react";

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
    setError,
  } = useForm<CreateLocationFormValues>({
    defaultValues: defaultData,
    resolver: zodResolver(createLocationSchema),
  });

  const { createLocation, isPending } = useCreateLocation({ setError });

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
      <DialogContent className="sm:max-w-150 w-[95vw] max-h-[90vh] overflow-y-auto">
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
            <div
              className={`flex flex-col gap-4" ${
                errors._addressGroupError
                  ? "border-destructive bg-destructive/10" // Красная рамка при ошибке группы
                  : "border-border"
              }`}
            >
              {errors._addressGroupError && (
                <p className="text-sm text-destructive font-medium mb-3 flex items-center gap-2">
                  <AlertCircle className="h-4 w-4" />
                  {errors._addressGroupError.message}
                </p>
              )}
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
            <div className="flex gap-4">
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
            </div>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
