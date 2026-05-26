import { locationsQueryOptions } from "@/entities/locations/api";
import { EnvelopeError } from "@/shared/api/error";
import { useQuery } from "@tanstack/react-query";

const PAGE_SIZE = 10;
export function useLocationsLists({ page }: { page: number }) {
  const {
    data: data,
    isPending,
    error,
    isError,
  } = useQuery(
    locationsQueryOptions.getLocationsOptions({ page, pageSize: PAGE_SIZE }),
  );

  return {
    locations: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    currentPage: data?.page,
    isPending,
    error: error instanceof EnvelopeError ? error : undefined,
    isError: isError,
  };
}
