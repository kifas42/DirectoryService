import { ApiEnvelope, PaginationResponse } from "@/shared/api/types";
import { GetLocationDto, GetLocationRequest } from "./types";
import { apiClient } from "@/shared/api/axios";

export const locationsApi = {
  getLocations: async (request: GetLocationRequest) => {
    const response = await apiClient.get<
      ApiEnvelope<PaginationResponse<GetLocationDto>>
    >("/locations", {
      params: request,
    });

    return response.data.result;
  },
};
