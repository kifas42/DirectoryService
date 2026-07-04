import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { UseFormSetError } from "react-hook-form";
import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { queryClient } from "@/shared/api/query-client";
import { handleApiError } from "@/shared/api/handle-api-error";
import { LocationFormValues } from "./types";
import { locationErrorMap } from "../location-error-map";

interface UseUpdateLocationOptions {
  setError?: UseFormSetError<LocationFormValues>;
}

export function useUpdateLocation(options?: UseUpdateLocationOptions) {
  const mutation = useMutation({
    mutationFn: locationsApi.updateLocation,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      });
    },
    onSuccess: () => {
      toast.success("Локация успешно изменена");
    },
    onError: (error) => {
      if (options?.setError) {
        handleApiError(error, options.setError, locationErrorMap);
      } else {
        toast.error(
          error instanceof Error
            ? error.message
            : "Ошибка при обновлении локации",
        );
      }
    },
  });

  return {
    updateLocation: mutation.mutate,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
