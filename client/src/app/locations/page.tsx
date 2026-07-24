"use client";

import { GetLocationDto } from "@/entities/locations/types";
import { LocationTableSkeleton } from "@/entities/locations/ui/table.skeleton";
import { CreateLocationDialog } from "@/features/locations/create-location-dialog";
import { DataTablePagination } from "@/shared/components/data-table-pagination";
import { EditLocationDialog } from "@/features/locations/edit-location-dialog";
import { FilterBar } from "@/features/locations/filter-bar";
import { LocationTable } from "@/features/locations/location-table";
import { useLocationsLists } from "@/features/locations/model/use-locations-list";
import { QueryErrorAlert } from "@/shared/components/query-error-alert";
import { Button } from "@/shared/components/ui/button";
import { Plus } from "lucide-react";
import { useState } from "react";
import { useDataFilters } from "@/hooks/use-data-filters";
import useDeleteLocation from "@/features/locations/model/use-delete-location";
import DeleteLocationDialog from "@/features/locations/delete-location-dialog";

const PAGE_SIZE = 8;

export default function Locations() {
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);

  const [selectedLocation, setSelectedLocation] =
    useState<GetLocationDto | null>(null);

  const filters = useDataFilters({
    initialPageSize: PAGE_SIZE,
  });

  const handleEditClick = (location: GetLocationDto) => {
    setSelectedLocation({
      ...location,
      stateOrProvince: location.stateOrProvince ?? undefined,
      postalCode: location.postalCode ?? undefined,
    });
    setEditOpen(true);
  };

  const handleDeleteClick = (location: GetLocationDto) => {
    setSelectedLocation(location);
    setDeleteOpen(true);
  };

  const {
    locations,
    isPending,
    error,
    totalPages,
    isError,
    isPlaceholderData,
  } = useLocationsLists(filters.apiParams);

  const { deleteLocation, isPending: isDeletePending } = useDeleteLocation();

  const handleConfirmDelete = () => {
    if (!selectedLocation) return;

    deleteLocation(selectedLocation);
    setDeleteOpen(false);
    setSelectedLocation(null);
  };

  if (isError && error) {
    console.error(error);
  }

  return (
    <>
      <div className="mb-6">
        <FilterBar
          search={filters.search}
          onSearchChange={filters.handleSearchChange}
          sortBy={filters.sortBy}
          onSortByChange={filters.handleSortByChange}
          sortOrder={filters.sortOrder}
          onSortOrderChange={filters.handleSortOrderChange}
          isActive={filters.isActive}
          onIsActiveChange={filters.handleIsActiveChange}
        >
          <Button
            size="lg"
            className="shadow-md hover:shadow-lg transition-shadow"
            onClick={() => setCreateOpen(true)}
          >
            <Plus className="mr-2 h-5 w-5" />
            Создать новую локацию
          </Button>
        </FilterBar>
      </div>
      <div
        className={`transition-all duration-300 ease-in-out ${
          isPlaceholderData
            ? "opacity-40 blur-[2px] translate-y-1 scale-[0.99] pointer-events-none"
            : "opacity-100 blur-0 translate-y-0 scale-100"
        }`}
      >
        {isPending && (
          <LocationTableSkeleton rows={filters.apiParams.pageSize} />
        )}

        {isError && (
          <QueryErrorAlert
            message={error ? error.message : "Не удалось загрузить данные"}
            queryKey={["locations"]}
          />
        )}

        {locations && (
          <LocationTable
            locations={locations}
            onEdit={handleEditClick}
            onDelete={handleDeleteClick}
          />
        )}

        <DataTablePagination
          currentPage={filters.currentPage}
          totalPages={totalPages ?? 1}
          onPageChange={filters.setPage}
          onNextPage={() => filters.handleNextPage(totalPages ?? 1)}
          onPrevPage={filters.handlePrevPage}
        />
      </div>

      <CreateLocationDialog open={createOpen} onOpenChange={setCreateOpen} />

      {selectedLocation && (
        <EditLocationDialog
          key={`edit-${selectedLocation.id}`}
          open={editOpen}
          onOpenChange={setEditOpen}
          location={selectedLocation}
          resetSelected={() => setSelectedLocation(null)}
        />
      )}
      {selectedLocation && deleteOpen && (
        <DeleteLocationDialog
          key={`delete-${selectedLocation.id}`}
          open={deleteOpen}
          onOpenChange={setDeleteOpen}
          location={selectedLocation}
          onConfirm={handleConfirmDelete}
          isPending={isDeletePending}
        />
      )}
    </>
  );
}
