import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { LocationFormValues, locationSchema } from "./model/types";
import { LocationForm } from "./location-form";
import {
  EditLocationRequest,
  GetLocationDto,
} from "@/entities/locations/types";
import { useUpdateLocation } from "./model/use-update-location";

interface EditLocationDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  location: GetLocationDto;
}

export function EditLocationDialog({
  open,
  onOpenChange,
  location,
}: EditLocationDialogProps) {
  const {
    control,
    handleSubmit,
    formState: { errors },
    reset,
    setError,
  } = useForm<LocationFormValues>({
    resolver: zodResolver(locationSchema),
    defaultValues: location,
  });
  const { updateLocation, isPending } = useUpdateLocation({ setError });
  const onSubmit = ({
    name,
    timezone,
    ...addressFields
  }: LocationFormValues) => {
    const request: EditLocationRequest = {
      id: location.id,
      name,
      timezone,
      address: addressFields,
    };

    updateLocation(request, {
      onSuccess: () => {
        onOpenChange(false);
        reset();
      },
    });
  };

  return (
    <LocationForm
      open={open}
      onOpenChange={onOpenChange}
      title="Редактирование локации"
      submitText="Сохранить"
      isPending={isPending}
      onSubmit={handleSubmit(onSubmit)}
      onReset={() => reset()}
      control={control}
      errors={errors}
    />
  );
}
