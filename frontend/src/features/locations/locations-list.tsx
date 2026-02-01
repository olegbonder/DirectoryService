"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { Location } from "@/entities/locations/types";
import LocationFilters from "./locations-filters";
import { EnvelopeError } from "@/shared/api/errors";
import { useLocationsList } from "./model/use-locations-list";
import { Button } from "@/shared/components/ui/button";
import LocationCard from "./location-card";
import CreateLocationDialog from "./create-location-dialog";
import DeleteLocationAlertDialog from "./delete-location-alert";
import UpdateLocationDialog from "./update-location-dialog";
import {
  setLocationsPage,
  useGetLocationsFilter,
} from "./model/locations-filters-store";
import { useGetGlobalFilter } from "@/shared/stores/global-search-store";

export default function LocationList() {
  const globalSearch = useGetGlobalFilter();
  const { search, departmentIds, isActive, page, pageSize, order } =
    useGetLocationsFilter();

  const {
    locations,
    isPending,
    error,
    isError,
    isFetchingNextPage,
    cursorRef,
  } = useLocationsList({
    departmentIds,
    search: search === "" ? globalSearch : search,
    isActive,
    pageSize,
    page: departmentIds.length === 0 ? page : 1,
    order,
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
    <div className="container mx-auto px-4 py-8">
      <div className="mb-12">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-4xl font-bold bg-linear-to-r from-emerald-600 to-cyan-600 bg-clip-text text-transparent mb-2">
              Локации
            </h1>
            <p className="text-slate-600">
              Управление местоположениями в организации
            </p>
          </div>
          {!isError && (
            <Button
              onClick={() => setCreateOpen(true)}
              className="bg-linear-to-r from-emerald-600 to-emerald-700 hover:from-emerald-700 hover:to-emerald-800 text-white font-semibold px-6 py-3 h-auto rounded-lg shadow-lg hover:shadow-xl transition-all duration-300 transform hover:scale-105"
            >
              <span className="mr-2 text-lg">+</span>
              Создать локацию
            </Button>
          )}
        </div>
        {isError && (
          <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-800">
            <p className="font-semibold">Ошибка загрузки</p>
            <p className="text-sm mt-1">{getError(error!)}</p>
          </div>
        )}
      </div>

      {!isError && (
        <div className="mb-6 p-4 bg-white rounded-lg shadow-sm border border-slate-200">
          <LocationFilters />
        </div>
      )}

      {isPending && (
        <div className="flex justify-center items-center py-16">
          <div className="text-center">
            <Spinner />
            <p className="text-slate-400 mt-4">Загрузка локаций...</p>
          </div>
        </div>
      )}

      {!isPending && (!locations || locations.length === 0) && (
        <div className="flex flex-col items-center justify-center py-20">
          <div className="text-slate-400 text-center">
            <p className="text-lg font-medium mb-2">Локации не найдены</p>
          </div>
        </div>
      )}

      {!isPending && locations && locations.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12">
          {locations.map((location) => (
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
      )}
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
