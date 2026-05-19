import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Spinner } from "@/shared/components/ui/spinner";
import { SubmitEvent, useState } from "react";
import { useCreateLocation } from "./model/use-create-location";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/shared/components/ui/field";
import { Input } from "@/shared/components/ui/input";
import { AddressDto, CreateLocationRequest } from "@/entities/locations/types";

export function CreateLocationDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const defaultData: CreateLocationRequest = {
    name: "",
    address: {
      officeNumber: "",
      buildingNumber: "",
      street: "",
      city: "",
      stateOrProvince: null,
      country: "",
      postalCode: null,
    },
    timezone: "Europe/Moscow",
  };

  const [formData, setFormData] = useState<CreateLocationRequest>(defaultData);

  const { createLocation, isPending, isError, error } = useCreateLocation();

  const handleSubmit = (e: SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    createLocation(formData, {
      onSuccess: () => {
        setFormData(defaultData);
        onOpenChange(false);
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-187.5">
        <form onSubmit={handleSubmit}>
          <DialogTitle>Создание локации</DialogTitle>
          <DialogDescription>заполните форму</DialogDescription>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 py-4">
            <div className="flex flex-col gap-4">
              <FieldGroup>
                <Field>
                  <FieldLabel htmlFor="name">Название</FieldLabel>
                  <Input
                    id="name"
                    placeholder="Название"
                    required
                    value={formData.name}
                    onChange={(e) =>
                      setFormData({ ...formData, name: e.target.value })
                    }
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="timezone">Часовой пояс</FieldLabel>
                  <Input
                    id="timezone"
                    placeholder="Europe/Moscow"
                    required
                    value={formData.timezone}
                    onChange={(e) =>
                      setFormData({ ...formData, timezone: e.target.value })
                    }
                  />
                </Field>
              </FieldGroup>
            </div>

            <div className="flex flex-col gap-4">
              <FieldGroup>
                <Field>
                  <FieldLabel htmlFor="officeNumber">Номер офиса</FieldLabel>
                  <Input
                    id="officeNumber"
                    placeholder="A12"
                    required
                    value={formData.address.officeNumber}
                    onChange={(e) => {
                      const addr: AddressDto = {
                        ...formData.address,
                        officeNumber: e.target.value,
                      };
                      setFormData({ ...formData, address: addr });
                    }}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="buildingNumber">Номер дома</FieldLabel>
                  <Input
                    id="buildingNumber"
                    placeholder="45"
                    required
                    value={formData.address.buildingNumber}
                    onChange={(e) => {
                      const addr: AddressDto = {
                        ...formData.address,
                        buildingNumber: e.target.value,
                      };
                      setFormData({ ...formData, address: addr });
                    }}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="street">Улица</FieldLabel>
                  <Input
                    id="street"
                    placeholder="Тверская"
                    required
                    value={formData.address.street}
                    onChange={(e) => {
                      const addr: AddressDto = {
                        ...formData.address,
                        street: e.target.value,
                      };
                      setFormData({ ...formData, address: addr });
                    }}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="city">Город</FieldLabel>
                  <Input
                    id="city"
                    placeholder="Москва"
                    required
                    value={formData.address.city}
                    onChange={(e) => {
                      const addr: AddressDto = {
                        ...formData.address,
                        city: e.target.value,
                      };
                      setFormData({ ...formData, address: addr });
                    }}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="stateOrProvince">Регион</FieldLabel>
                  <Input
                    id="stateOrProvince"
                    placeholder="Москва"
                    value={formData.address.stateOrProvince ?? ""}
                    onChange={(e) => {
                      const addr: AddressDto = {
                        ...formData.address,
                        stateOrProvince: e.target.value,
                      };
                      setFormData({ ...formData, address: addr });
                    }}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="country">Страна</FieldLabel>
                  <Input
                    id="country"
                    placeholder="РФ"
                    required
                    value={formData.address.country}
                    onChange={(e) => {
                      const addr: AddressDto = {
                        ...formData.address,
                        country: e.target.value,
                      };
                      setFormData({ ...formData, address: addr });
                    }}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="postalCode">Индекс</FieldLabel>
                  <Input
                    id="postalCode"
                    placeholder="11144"
                    required
                    value={formData.address.postalCode ?? ""}
                    onChange={(e) => {
                      const addr: AddressDto = {
                        ...formData.address,
                        postalCode: e.target.value,
                      };
                      setFormData({ ...formData, address: addr });
                    }}
                  />
                </Field>
              </FieldGroup>
            </div>
          </div>

          <DialogFooter>
            <Field orientation="horizontal">
              <Button
                variant="outline"
                onClick={() => setFormData(defaultData)}
                disabled={isPending}
              >
                Сброс
              </Button>
              <Button type="submit" disabled={isPending}>
                Создать
                {isPending && <Spinner />}
              </Button>
            </Field>
            {error && (
              <div className="text-red-500 text-sm mt-2">{error.message}</div>
            )}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
