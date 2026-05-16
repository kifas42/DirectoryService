"use client";

import { locationsApi } from "@/entities/locations/api";
import { LocationTableSkeleton } from "@/entities/locations/ui/table.skeleton";
import { LocationTable } from "@/features/location-list/locations";
import { queryClient } from "@/shared/api/query-client";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import { useQuery } from "@tanstack/react-query";
import { AlertCircle, RefreshCw } from "lucide-react";
import { useState } from "react";

const PAGE_SIZE = 10;

export default function Locations() {
  const [page, setPage] = useState(1);
  const {
    data: data,
    isPending,
    error,
  } = useQuery({
    queryFn: () =>
      locationsApi.getLocations({ page: page, pageSize: PAGE_SIZE }),
    queryKey: ["locations", page, PAGE_SIZE],
  });

  if (isPending) {
    return <LocationTableSkeleton rows={5} />;
  }

  if (error) {
    const message =
      error instanceof Error ? error.message : "Не удалось загрузить данные";

    return (
      <Alert variant="destructive" className="max-w-lg">
        <AlertCircle className="h-4 w-4" />
        <AlertTitle>Ошибка загрузки</AlertTitle>
        <AlertDescription className="mt-2 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <span>{message}</span>
          <Button
            variant="outline"
            size="sm"
            className="bg-transparent hover:bg-destructive/20 text-destructive border-destructive/50"
            onClick={() =>
              queryClient.invalidateQueries({ queryKey: ["locations"] })
            }
          >
            <RefreshCw className="mr-2 h-3 w-3" />
            Повторить
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  if (!data) {
    return (
      <div className="flex h-24 items-center justify-center rounded-md border text-muted-foreground">
        Список локаций пуст
      </div>
    );
  }

  return <LocationTable locations={data.items} />;
}
