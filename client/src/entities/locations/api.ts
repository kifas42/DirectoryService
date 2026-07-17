import { PaginationResponse } from "@/shared/api/types";
import {
  CreateLocationRequest,
  DeleteLocationRequest,
  EditLocationRequest,
  GetLocationDto,
  GetLocationRequest,
} from "./types";
import { apiClient } from "@/shared/api/axios";
import { queryOptions } from "@tanstack/react-query";
import { ApiEnvelope } from "@/shared/api/envelope";
import { DataTableFilterParams } from "@/features/locations/model/types";

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

  deleteLocation: async (request: DeleteLocationRequest) => {
    const response = await apiClient.delete<ApiEnvelope>(
      `/locations/${request.id}`,
    );

    return response.data.result;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",
  getLocationsOptions: (params: DataTableFilterParams) => {
    return queryOptions({
      queryFn: () => locationsApi.getLocations(params),
      queryKey: [locationsQueryOptions.baseKey, ...Object.values(params)],
    });
  },
};
