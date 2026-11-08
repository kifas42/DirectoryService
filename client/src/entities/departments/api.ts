import { ApiEnvelope } from "@/shared/api/envelope";
import { PaginationResponse } from "@/shared/api/types";
import { GetDepartmentDto, GetDepartmentsRequest } from "./types";
import { apiClient } from "@/shared/api/axios";
import { queryOptions } from "@tanstack/react-query";

export const departmentsApi = {
  getDepartments: async (request: GetDepartmentsRequest) => {
    const response = await apiClient.get<
      ApiEnvelope<PaginationResponse<GetDepartmentDto>>
    >("/departments", {
      params: request,
      paramsSerializer: {
        indexes: null,
      },
    });

    return response.data.result;
  },
};

export const departmentsQueryOptions = {
  baseKey: "departments",
  getDepartmentsOptions: (params: GetDepartmentsRequest) => {
    return queryOptions({
      queryFn: () => departmentsApi.getDepartments(params),
      queryKey: [departmentsQueryOptions.baseKey, params],
    });
  },
};
