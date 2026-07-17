import { locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/error";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { DataTableFilterParams } from "./types";

export function useLocationsLists(params: DataTableFilterParams) {
  const { data, isPending, error, isError, isPlaceholderData } = useQuery({
    ...locationsQueryOptions.getLocationsOptions(params),
    placeholderData: keepPreviousData,
  });

  return {
    locations: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    currentPage: data?.page,
    isPending,
    error: error instanceof EnvelopeError ? error : undefined,
    isError: isError,
    isPlaceholderData,
  };
}
