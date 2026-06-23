import { positionsQueryOptions } from "@/entities/positions/api";
import { EnvelopeError } from "@/shared/api/error";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";

const PAGE_SIZE = 2;
export function usePositionsLists() {
  const {
    data,
    isPending,
    error,
    isError,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
    isFetchNextPageError,
  } = useInfiniteQuery({
    ...positionsQueryOptions.getPositionInfiniteOptions({
      pageSize: PAGE_SIZE,
      sortBy: "name",
      sortOrder: "asc",
    }),
  });

  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        {
          threshold: 0.5,
        },
      );

      if (el) {
        observer.observe(el);

        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage],
  );

  return {
    positions: data ?? [],
    isPending,
    error: error instanceof EnvelopeError ? error : undefined,
    isError: isError,
    refetch,
    isFetchingNextPage,
    cursorRef,
    hasNextPage,
    fetchNextPage,
    isErrorNextPage: isFetchNextPageError,
  };
}
