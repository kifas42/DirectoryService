import { EnvelopeError } from "@/shared/api/error";
import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { departmentsQueryOptions } from "@/entities/departments/api";
import { GetDepartmentsRequest } from "@/entities/departments/types";

export function useDepartmentsLists(params: GetDepartmentsRequest) {
  const { data, isPending, isFetching, error, isError, isPlaceholderData } = useQuery({
    ...departmentsQueryOptions.getDepartmentsOptions(params),
    placeholderData: keepPreviousData,
  });

  return {
    departments: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    currentPage: data?.page,
    isPending,
    isFetching,
    error: error instanceof EnvelopeError ? error : undefined,
    isError: isError,
    isPlaceholderData,
  };
}
