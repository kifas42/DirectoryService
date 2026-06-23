import { InfinityScrollResponse } from "@/shared/api/types";
import {
  CreatePositionRequest,
  GetPositionDto,
  GetPositionRequest,
} from "./types";
import { apiClient } from "@/shared/api/axios";
import { infiniteQueryOptions } from "@tanstack/react-query";
import { ApiEnvelope } from "@/shared/api/envelope";

export const positionsApi = {
  getPositions: async (request: GetPositionRequest) => {
    const response = await apiClient.get<
      ApiEnvelope<InfinityScrollResponse<GetPositionDto>>
    >("/positions", {
      params: request,
    });

    return response.data.result;
  },

  createPosition: async (request: CreatePositionRequest) => {
    const response = await apiClient.post<ApiEnvelope<string>>(
      "/positions",
      request,
    );

    return response.data.result;
  },
};

export const positionsQueryOptions = {
  baseKey: "positions",
  getPositionInfiniteOptions: ({
    pageSize,
    sortBy,
    sortOrder,
  }: {
    pageSize: number;
    sortBy: string;
    sortOrder: string;
  }) => {
    return infiniteQueryOptions({
      queryKey: [
        positionsQueryOptions.baseKey,
        "infinite",
        pageSize,
        sortBy,
        sortOrder,
      ],
      queryFn: ({ pageParam }) => {
        return positionsApi.getPositions({
          cursor: pageParam ?? undefined,
          limit: pageSize,
          sortBy,
          sortOrder,
        });
      },

      initialPageParam: undefined as string | undefined,

      getNextPageParam: (lastPage) => {
        return lastPage?.nextCursor ?? undefined;
      },
      select: (data): GetPositionDto[] => {
        return data.pages.flatMap((page) => page?.items ?? []);
      },
    });
  },
};
