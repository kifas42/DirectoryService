"use client";

import { usePositionsLists } from "@/features/positions/model/use-position-list";
import PositionsList from "@/features/positions/positions-list";
import { QueryErrorAlert } from "@/shared/components/query-error-alert";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";

export default function Positions() {
  const {
    positions,
    isPending,
    error,
    isError,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
    cursorRef,
    isErrorNextPage,
  } = usePositionsLists();

  if (isError && (!positions || positions.length === 0)) {
    return (
      <QueryErrorAlert
        message={error ? error.message : "Не удалось загрузить данные"}
        queryKey={["positions"]}
      />
    );
  }

  return (
    <>
      {isPending && <Spinner />}
      {positions && <PositionsList positions={positions} />}
      <div
        ref={cursorRef}
        className="flex flex-col items-center justify-center py-6 gap-2"
      >
        {isFetchingNextPage && <Spinner />}

        {isErrorNextPage && !isFetchingNextPage && (
          <div className="flex flex-col items-center gap-2 text-center bg-destructive/5 border border-destructive/20 p-4 rounded-lg w-full max-w-md mx-auto">
            <p className="text-sm text-destructive font-medium">
              Не удалось загрузить следующие позиции
            </p>
            <Button
              variant="outline"
              size="sm"
              onClick={() => fetchNextPage()}
              className="h-8 text-xs"
            >
              Повторить попытку
            </Button>
          </div>
        )}

        {/* Конец списка */}
        {!hasNextPage && positions && positions.length > 0 && (
          <span className="text-sm font-medium text-muted-foreground/60 bg-muted/40 px-3 py-1 rounded-full">
            Вы просмотрели все позиции
          </span>
        )}
      </div>
    </>
  );
}
