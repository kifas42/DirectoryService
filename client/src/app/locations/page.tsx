"use client";

import { GetLocationDto } from "@/entities/locations/types";
import { LocationTableSkeleton } from "@/entities/locations/ui/table.skeleton";
import { CreateLocationDialog } from "@/features/locations/create-location-dialog";
import { EditLocationDialog } from "@/features/locations/edit-location-dialog";
import { LocationTable } from "@/features/locations/location-table";
import { useLocationsLists } from "@/features/locations/model/use-locations-list";
import { QueryErrorAlert } from "@/shared/components/query-error-alert";
import { Button } from "@/shared/components/ui/button";
import { Plus } from "lucide-react";
import { useState } from "react";

export default function Locations() {
  const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [selectedLocation, setSelectedLocation] =
    useState<GetLocationDto | null>(null);

  const { locations, isPending, error, totalPages, totalCount, isError } =
    useLocationsLists({ page });

  const handleEditClick = (location: GetLocationDto) => {
    setSelectedLocation(location);
    setEditOpen(true);
  };

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
        size="lg"
        className="mb-6 shadow-md hover:shadow-lg transition-shadow"
        onClick={() => setOpen(true)}
      >
        <Plus className="mr-2 h-5 w-5" />
        Создать новую локацию
      </Button>
      {isPending && <LocationTableSkeleton rows={5} />}
      {locations && (
        <LocationTable locations={locations} onEdit={handleEditClick} />
      )}
      <CreateLocationDialog open={open} onOpenChange={setOpen} />
      {selectedLocation && (
        <EditLocationDialog
          key={selectedLocation.id}
          open={editOpen}
          onOpenChange={setEditOpen}
          location={selectedLocation}
        />
      )}
    </>
  );
}
