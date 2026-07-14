import { PaginationResponse } from "@/shared/api/types";
import {
  CreateLocationRequest,
  EditLocationRequest,
  GetLocationDto,
  GetLocationRequest,
} from "./types";
import { apiClient } from "@/shared/api/axios";
import { queryOptions } from "@tanstack/react-query";
import { ApiEnvelope } from "@/shared/api/envelope";

export const locationsApi = {
  getLocations: async (request: GetLocationRequest) => {
    const response = await apiClient.get<
      ApiEnvelope<PaginationResponse<GetLocationDto>>
    >("/locations", {
      params: request,
    });

    return response.data.result;
  },

  createLocation: async (request: CreateLocationRequest) => {
    const response = await apiClient.post<ApiEnvelope<string>>(
      "/locations",
      request,
    );

    return response.data.result;
  },

  updateLocation: async (request: EditLocationRequest) => {
    const response = await apiClient.put<ApiEnvelope>(
      `/locations/${request.id}`,
      request,
    );

    return response.data.result;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",
  getLocationsOptions: ({
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
  }) => {
    return queryOptions({
      queryFn: () =>
        locationsApi.getLocations({
          page: page,
          search: search,
          pageSize: pageSize,
          sortBy: sortBy,
          sortOrder: sortOrder,
        }),
      queryKey: [
        locationsQueryOptions.baseKey,
        page,
        pageSize,
        sortBy,
        sortOrder,
        search,
      ],
    });
  },
};
