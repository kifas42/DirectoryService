import { locationsApi, locationsQueryOptions } from "@/entities/locations/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export default function useDeleteLocation() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: locationsApi.deleteLocation,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [locationsQueryOptions.baseKey],
      });
    },
    onSuccess: () => {
      toast.success("Локация успешно удалена");
    },
    onError: (error) => {
      toast.error(
        error instanceof Error ? error.message : "Ошибка при удалении локации",
      );
    },
  });

  return {
    deleteLocation: mutation.mutate,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
