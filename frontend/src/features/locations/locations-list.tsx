"use client";

import { locationsApi } from "@/entities/locations/api";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Spinner } from "@/shared/components/ui/spinner";
import { MapPin } from "lucide-react";
import { useState } from "react";
import useFilterLocations from "@/shared/hooks/use-filter-locations";
import { useQuery } from "@tanstack/react-query";
import LocationFilters from "./locations-filters";
import LocationsPagination from "./locations-pagination";
import { EnvelopeError } from "@/shared/api/errors";

const PAGE_SIZE = 3;

export default function LocationList() {
  const {
    departmentIds,
    setDepartmentIds,
    search,
    setSearch,
    isActive,
    setIsActive,
  } = useFilterLocations();
  const [page, setPage] = useState(1);
  const {
    data,
    isPending: getIsPending,
    error: error,
    isError,
  } = useQuery({
    queryFn: () =>
      locationsApi.getLocations({
        page,
        pageSize: PAGE_SIZE,
        departmentIds,
        search,
        isActive,
      }),
    queryKey: ["locations", page, departmentIds, search, isActive],
  });

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
          <div className="text-red-500 mb-4">Ошибка: {getError(error)}</div>
        )}
        <LocationFilters
          departmentIds={departmentIds}
          setDepartmentIds={setDepartmentIds}
          search={search}
          setSearch={setSearch}
          isActive={isActive}
          setIsActive={setIsActive}
        />
      </div>
      {getIsPending && <Spinner />}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {data?.items?.map((location) => (
          <Card key={location.id} className="hover:shadow-lg transition-shadow">
            <CardHeader className="flex items-center justify-between pb-3">
              <CardTitle className="text-lg font-semibold">
                {location.name}
              </CardTitle>
              <Checkbox checked={location.isActive} disabled />
            </CardHeader>
            <CardContent className="space-y-2">
              <div className="flex items-start gap-2">
                <MapPin className="h-4 w-4 text-gray-500 shrink-0" />
                <div className="text-sm text-gray-700">
                  <p>
                    {location.street} {location.houseNumber}
                    {location.flatNumber ? `, кв. ${location.flatNumber}` : ""}
                  </p>
                  <p>
                    {location.city}, {location.country}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
      {data && data.totalPages > 1 && (
        <LocationsPagination
          totalPages={data.totalPages}
          page={page}
          setPage={setPage}
        />
      )}
    </div>
  );
}
