import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/ui/dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import {
  Field,
  FieldLabel,
  FieldError,
  FieldGroup,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { AlertCircle } from "lucide-react";
import { Control, Controller, FieldErrors } from "react-hook-form";
import { LocationFormValues } from "./model/types";

interface LocationFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  submitText: string;
  isPending: boolean;
  onSubmit: (e?: React.BaseSyntheticEvent) => void;
  onReset: () => void;
  control: Control<LocationFormValues>;
  errors: FieldErrors<LocationFormValues>;
}

export function LocationForm({
  open,
  onOpenChange,
  title,
  submitText,
  isPending,
  onSubmit,
  onReset,
  control,
  errors,
}: LocationFormProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-150 w-[95vw] max-h-[90vh] overflow-y-auto">
        <form onSubmit={onSubmit}>
          <DialogTitle>{title}</DialogTitle>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 py-4">
            {/* Левая колонка */}
            <div className="flex flex-col gap-4">
              <FieldGroup>
                <Controller
                  name="name"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-name">Название</FieldLabel>
                      <Input
                        {...field}
                        id="location-name"
                        placeholder="Название"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />

                <Controller
                  name="timezone"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-timezone">
                        Часовой пояс
                      </FieldLabel>
                      <Input
                        {...field}
                        id="location-timezone"
                        placeholder="Europe/Moscow"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />
              </FieldGroup>
            </div>

            {/* Правая колонка */}
            <div
              className={`flex flex-col gap-4 ${
                errors._addressGroupError
                  ? "border-destructive bg-destructive/10"
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
                <Controller
                  name="officeNumber"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-officeNumber">
                        Номер офиса
                      </FieldLabel>
                      <Input
                        {...field}
                        id="location-officeNumber"
                        placeholder="A12"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />

                <Controller
                  name="buildingNumber"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-buildingNumber">
                        Номер дома
                      </FieldLabel>
                      <Input
                        {...field}
                        id="location-buildingNumber"
                        placeholder="45"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />

                <Controller
                  name="street"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-street">Улица</FieldLabel>
                      <Input
                        {...field}
                        id="location-street"
                        placeholder="Тверская"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />

                <Controller
                  name="city"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-city">Город</FieldLabel>
                      <Input
                        {...field}
                        id="location-city"
                        placeholder="Москва"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />

                <Controller
                  name="stateOrProvince"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-stateOrProvince">
                        Регион
                      </FieldLabel>
                      <Input
                        {...field}
                        value={field.value ?? ""} // Предотвращаем ошибку null/undefined
                        id="location-stateOrProvince"
                        placeholder="Москва"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />

                <Controller
                  name="country"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-country">Страна</FieldLabel>
                      <Input
                        {...field}
                        id="location-country"
                        placeholder="РФ"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />

                <Controller
                  name="postalCode"
                  control={control}
                  render={({ field, fieldState }) => (
                    <Field data-invalid={fieldState.invalid}>
                      <FieldLabel htmlFor="location-postalCode">
                        Индекс
                      </FieldLabel>
                      <Input
                        {...field}
                        value={field.value ?? ""} // Предотвращаем ошибку null/undefined
                        id="location-postalCode"
                        placeholder="11144"
                        autoComplete="off"
                      />
                      {fieldState.invalid && (
                        <FieldError errors={[fieldState.error]} />
                      )}
                    </Field>
                  )}
                />
              </FieldGroup>
            </div>
          </div>

          <DialogFooter className="flex flex-col sm:flex-row gap-4 items-center justify-between w-full">
            <div className="flex gap-4">
              <Button
                type="button"
                variant="outline"
                onClick={onReset}
                disabled={isPending}
              >
                Сброс
              </Button>
              <Button type="submit" disabled={isPending}>
                {submitText}
                {isPending && <Spinner className="ml-2" />}
              </Button>
            </div>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
