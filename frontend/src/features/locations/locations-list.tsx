"use client";

import { locationsApi } from "@/entities/locations/api";
import { LocationDTO } from "@/entities/locations/types";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { Spinner } from "@/shared/components/ui/spinner";
import { MapPin } from "lucide-react";
import { useEffect, useState } from "react";

const PAGE_SIZE = 20;

export default function LocationList() {
  const [locations, setLocations] = useState<LocationDTO[]>([]);
  const [page, setPage] = useState(1);
  const [departmentIds, setDepartmentIds] = useState<string[] | undefined>(
    undefined
  );
  const [search, setSearch] = useState<string | undefined>(undefined);
  const [isActive, setIsActive] = useState<boolean>(true);

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    locationsApi
      .getLocations({
        page,
        pageSize: PAGE_SIZE,
        departmentIds,
        search,
        isActive,
      })
      .then((data) => setLocations(data.locations))
      .catch((error) => setError(error.message))
      .finally(() => setIsLoading(false));
  }, [page, departmentIds, search, isActive]);

  if (isLoading) {
    return <Spinner />;
  }

  if (error) {
    return <div className="text-red-500">{error}</div>;
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <h1 className="text-3xl mb-2">Локации</h1>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {locations.map((location) => (
          <Card key={location.id} className="hover:shadow-lg transition-shadow">
            <CardHeader className="flex items-center justify-between pb-3">
              <CardTitle className="text-lg font-semibold">
                {location.name}
              </CardTitle>
              <Checkbox checked={location.isActive} disabled />
            </CardHeader>
            <CardContent className="space-y-2">
              <div className="flex items-start gap-2">
                <MapPin className="h-4 w-4 text-gray-500 flex-shrink-0" />
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
    </div>
  );
}
