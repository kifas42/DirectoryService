"use client";

import { LocationTableSkeleton } from "@/entities/locations/ui/table.skeleton";
import { CreateLocationDialog } from "@/features/locations/create-location-dialog";
import { LocationTable } from "@/features/locations/location-table";
import { useLocationsLists } from "@/features/locations/model/use-locations-list";
import { queryClient } from "@/shared/api/query-client";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/shared/components/ui/alert";
import { Button } from "@/shared/components/ui/button";
import { AlertCircle, RefreshCw } from "lucide-react";
import { useState } from "react";

export default function Locations() {
  const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false);

  const { locations, isPending, error, totalPages, totalCount, isError } =
    useLocationsLists({ page });

  if (isError) {
    const message = error ? error.message : "Не удалось загрузить данные";

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

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        className="bg-transparent hover:bg-primary/10 text-primary border-primary/30"
        onClick={() => setOpen(true)}
      >
        Создать
      </Button>
      {isPending && <LocationTableSkeleton rows={5} />}
      {locations && <LocationTable locations={locations} />}
      <CreateLocationDialog open={open} onOpenChange={setOpen} />
    </>
  );
}
