import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
} from "@/shared/components/ui/card";
import { Badge } from "@/shared/components/ui/badge";
import { Briefcase, Trash2 } from "lucide-react";
import { Position } from "@/entities/positions/types";
import Link from "next/link";
import { Button } from "@/shared/components/ui/button";

type PositionCardProps = {
  position: Position;
  onDelete?: () => void;
};

export default function PositionCard({
  position,
  onDelete,
}: PositionCardProps) {
  return (
    <Link href={`/positions/${position.id}`}>
      <Card className="group relative overflow-hidden border-0 bg-linear-to-br from-slate-50 to-slate-100 shadow-md hover:shadow-xl transition-all duration-300 hover:scale-[1.02]">
        {/* Decorative gradient background */}
        <div className="absolute inset-0 bg-linear-to-r from-blue-500/0 via-blue-500/0 to-purple-500/0 group-hover:from-blue-500/5 group-hover:via-blue-500/5 group-hover:to-purple-500/5 transition-all duration-300" />

        <CardHeader className="relative pb-4 pt-5 px-6">
          <div className="flex items-start justify-between gap-3">
            <div className="flex items-center gap-3 flex-1 min-w-0">
              <div className="p-2 rounded-lg bg-linear-to-br from-blue-500 to-blue-600 text-white shrink-0">
                <Briefcase className="h-5 w-5" />
              </div>
              <CardTitle className="text-base font-bold text-slate-900 line-clamp-3 flex-1">
                {position.name}
              </CardTitle>
            </div>
            <Badge
              variant={position.isActive ? "default" : "secondary"}
              className={`shrink-0 font-medium px-2.5 py-1 text-xs whitespace-nowrap ${
                position.isActive
                  ? "bg-emerald-500 hover:bg-emerald-600 text-white"
                  : "bg-slate-300 hover:bg-slate-400 text-slate-700"
              }`}
            >
              {position.isActive ? "Активна" : "Неактивна"}
            </Badge>
          </div>
        </CardHeader>

        <CardContent className="relative px-6 pb-5">
          <div className="space-y-4">
            {/* Description */}
            <div className="space-y-1">
              <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                Описание
              </p>
              <p className="text-sm text-slate-700 leading-relaxed">
                {position.description || "Нет описания"}
              </p>
            </div>

            {/* Departments count */}
            <div className="bg-white/60 rounded-lg p-3 border border-slate-200/50">
              <div className="flex items-center justify-between">
                <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                  Подразделения
                </p>
                <span className="font-bold text-slate-700 bg-white px-2.5 py-1 rounded border border-slate-300 text-sm">
                  {position.departmentsCount}
                </span>
              </div>
            </div>

            {/* Action buttons placeholder for alignment */}
            <div className="flex gap-2 pt-2 justify-end flex-wrap h-9">
              {position.isActive && onDelete && (
                <Button
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    onDelete();
                  }}
                  variant="ghost"
                  size="sm"
                  className="h-9 px-3 text-slate-600 hover:text-red-600 hover:bg-red-50 transition-colors font-medium"
                >
                  <Trash2 className="h-4 w-4 mr-2" />
                  Удалить
                </Button>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}
