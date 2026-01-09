import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
} from "@/shared/components/ui/card";
import { Checkbox } from "@radix-ui/react-checkbox";
import { MapPin } from "lucide-react";
import { Location } from "@/entities/locations/types";

type LocationCardProps = {
  location: Location;
};

export default function LocationCard({ location }: LocationCardProps) {
  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardHeader className="flex items-center justify-between pb-3">
        <CardTitle className="text-lg font-semibold">{location.name}</CardTitle>
        <Checkbox checked={location.isActive} disabled />
      </CardHeader>
      <CardContent className="space-y-2">
        <div className="flex items-start gap-2">
          <MapPin className="h-4 w-4 text-gray-500 shrink-0" />
          <div className="text-sm text-gray-700">
            <p>
              {location.street} {location.house}
              {location.flatNumber ? `, кв. ${location.flatNumber}` : ""}
            </p>
            <p>
              {location.city}, {location.country}
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
