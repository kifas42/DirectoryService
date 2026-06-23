"use client";

import { LocationTableSkeleton } from "@/entities/locations/ui/table.skeleton";
import { CreateLocationDialog } from "@/features/locations/create-location-dialog";
import { LocationTable } from "@/features/locations/location-table";
import { useLocationsLists } from "@/features/locations/model/use-locations-list";
import { QueryErrorAlert } from "@/shared/components/query-error-alert";
import { Button } from "@/shared/components/ui/button";
import { useState } from "react";

export default function Locations() {
  const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false);

  const { locations, isPending, error, totalPages, totalCount, isError } =
    useLocationsLists({ page });

  if (isError) {
    return (
      <QueryErrorAlert
        message={error ? error.message : "Не удалось загрузить данные"}
        queryKey={["locations"]}
      />
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
