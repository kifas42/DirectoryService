import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { queryClient } from "@/shared/api/query-client";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreateLocation() {
  const mutation = useMutation({
    mutationFn: locationsApi.createLocation,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      }),
    onError: () => {
      toast.error("Ошибка при создании");
    },

    onSuccess: () => {
      toast.success("Локация создана");
    },
  });

  return {
    createLocation: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
