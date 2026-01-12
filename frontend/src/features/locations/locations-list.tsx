"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { useFilterLocations } from "@/features/locations/model/use-filter-locations";
import { Location } from "@/entities/locations/types";
import LocationFilters from "./locations-filters";
import LocationsPagination from "./locations-pagination";
import { EnvelopeError } from "@/shared/api/errors";
import { useLocationsList } from "./model/use-locations-list";
import { Button } from "@/shared/components/ui/button";
import LocationCard from "./location-card";
import CreateLocationDialog from "./create-location-dialog";
import DeleteLocationAlertDialog from "./delete-location-alert";
import UpdateLocationDialog from "./update-location-dialog";

export default function LocationList() {
  const {
    departmentIds,
    setDepartmentIds,
    search,
    setSearch,
    isActive,
    setIsActive,
  } = useFilterLocations();
  const {
    locations,
    isPending,
    error,
    isError,
    isFetchingNextPage,
    cursorRef,
  } = useLocationsList({
    departmentIds,
    search,
    isActive,
  });
  const [createOpen, setCreateOpen] = useState(false);
  const [updateOpen, setUpdateOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [selectedLocation, setSelectedLocation] = useState<
    Location | undefined
  >(undefined);

  const getError = (error: Error): string => {
    return error instanceof EnvelopeError
      ? error.getAllMessages()
      : error.message;
  };

  return (
    <div className="container mx-auto px-4">
      <div className="mb-8">
        <h1 className="text-3xl mb-2">Локации</h1>
        {isError && (
          <div className="text-red-500 mb-4">Ошибка: {getError(error!)}</div>
        )}
        {!isError && (
          <div className="grid gap-4">
            <LocationFilters
              departmentIds={departmentIds}
              setDepartmentIds={setDepartmentIds}
              search={search}
              setSearch={setSearch}
              isActive={isActive}
              setIsActive={setIsActive}
            />
            <Button onClick={() => setCreateOpen(true)}>Создать локацию</Button>
          </div>
        )}
      </div>
      {isPending && (
        <div className="container mx-auto py-8 px-4 flex justify-center">
          <Spinner />
        </div>
      )}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {locations?.map((location) => (
          <LocationCard
            key={location.id}
            location={location}
            onEdit={() => {
              setSelectedLocation(location);
              setUpdateOpen(true);
            }}
            onDelete={() => {
              setSelectedLocation(location);
              setDeleteOpen(true);
            }}
          />
        ))}
      </div>
      <CreateLocationDialog open={createOpen} onOpenChange={setCreateOpen} />
      {selectedLocation && (
        <div>
          <UpdateLocationDialog
            key={selectedLocation.id}
            location={selectedLocation}
            open={selectedLocation !== undefined && updateOpen}
            onOpenChange={setUpdateOpen}
          />
          <DeleteLocationAlertDialog
            location={selectedLocation}
            open={deleteOpen}
            onOpenChange={setDeleteOpen}
          />
        </div>
      )}
      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
