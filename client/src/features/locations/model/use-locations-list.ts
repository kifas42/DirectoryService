import { locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/error";
import { keepPreviousData, useQuery } from "@tanstack/react-query";

export function useLocationsLists({
  page,
  pageSize,
  search,
  sortBy,
  sortOrder,
}: {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortOrder: "asc" | "desc";
}) {
  const { data, isPending, error, isError, isPlaceholderData } = useQuery({
    ...locationsQueryOptions.getLocationsOptions({
      page,
      pageSize,
      search,
      sortBy,
      sortOrder,
    }),
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
