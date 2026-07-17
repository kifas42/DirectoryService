import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { UseFormSetError } from "react-hook-form";
import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { handleApiError } from "@/shared/api/handle-api-error";
import { LocationFormValues } from "./types";
import { locationErrorMap } from "../location-error-map";

interface UseCreateLocationOptions {
  setError?: UseFormSetError<LocationFormValues>;
}

export function useCreateLocation(options?: UseCreateLocationOptions) {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.createLocation,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      });
    },
    onSuccess: () => {
      toast.success("Локация успешно создана");
    },
    onError: (error) => {
      if (options?.setError) {
        handleApiError(error, options.setError, locationErrorMap);
      } else {
        toast.error(
          error instanceof Error
            ? error.message
            : "Ошибка при создании локации",
        );
      }
    },
  });

  return {
    createLocation: mutation.mutate,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
