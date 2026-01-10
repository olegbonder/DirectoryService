import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
} from "@/shared/components/ui/card";
import { Checkbox } from "@/shared/components/ui/checkbox";
import { MapPin, Pencil, Trash2 } from "lucide-react";
import { Location } from "@/entities/locations/types";
import { Button } from "@/shared/components/ui/button";

type LocationCardProps = {
  location: Location;
  onEdit: () => void;
  onDelete: () => void;
};

export default function LocationCard({
  location,
  onEdit,
  onDelete,
}: LocationCardProps) {
  return (
    <Card className="hover:shadow-lg transition-shadow">
      <CardHeader className="flex items-center justify-between pb-3">
        <CardTitle className="text-lg font-semibold">{location.name}</CardTitle>
        <Checkbox checked={location.isActive} disabled />
      </CardHeader>
      <CardContent className="space-y-2">
        <div className="flex justify-between gap-2">
          <div className="flex gap-2">
            <MapPin className="h-4 w-4 text-gray-500 shrink-0" />
            <div className="text-sm text-gray-700">
              <p>
                {location.street} {location.house}
                {location.flat ? `, кв. ${location.flat}` : ""}
              </p>
              <p>
                {location.city}, {location.country}
              </p>
            </div>
          </div>
          <div className="flex gap-2">
            <Button
              onClick={onEdit}
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-muted-foreground hover:text-primary hover:bg-primary/10 transition-colors"
            >
              <Pencil className="h-4 w-4" />
            </Button>
            <Button
              onClick={onDelete}
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive hover:text-white! hover:bg-red-500! transition-colors"
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
