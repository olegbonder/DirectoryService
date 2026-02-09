import * as React from "react";
import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
} from "@/shared/components/ui/card";
import { Badge } from "@/shared/components/ui/badge";
import { Building, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Department } from "@/entities/departments/types";
import DepartmentPath from "./department-path";
import Link from "next/link";

type DepartmentCardProps = {
  department: Department;
  onEdit?: (department: Department) => void;
  onDelete?: () => void;
};

export default function DepartmentCard({
  department,
  onEdit,
  onDelete,
}: DepartmentCardProps) {
  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (onEdit) {
      onEdit(department);
    }
  };

  return (
    <Link href={`/departments/${department.id}`}>
      <Card className="group relative overflow-hidden border-0 bg-linear-to-br from-slate-50 to-slate-100 shadow-md hover:shadow-xl transition-all duration-300 hover:scale-[1.02]">
        {/* Decorative gradient background */}
        <div className="absolute inset-0 bg-linear-to-r from-emerald-500/0 via-emerald-500/0 to-cyan-500/0 group-hover:from-emerald-500/5 group-hover:via-emerald-500/5 group-hover:to-cyan-500/5 transition-all duration-300" />

        <CardHeader className="relative pb-4 pt-5 px-6">
          <div className="flex items-start justify-between gap-3">
            <div className="flex items-center gap-3 flex-1 min-w-0">
              <div className="p-2 rounded-lg bg-linear-to-br from-emerald-500 to-emerald-600 text-white shrink-0">
                <Building className="h-5 w-5" />
              </div>
              <CardTitle className="text-base font-bold text-slate-900 line-clamp-3 flex-1">
                {department.name}
              </CardTitle>
            </div>
            <Badge
              variant={department.isActive ? "default" : "secondary"}
              className={`shrink-0 font-medium px-2.5 py-1 text-xs whitespace-nowrap ${
                department.isActive
                  ? "bg-emerald-500 hover:bg-emerald-600 text-white"
                  : "bg-slate-300 hover:bg-slate-400 text-slate-700"
              }`}
            >
              {department.isActive ? "Активно" : "Неактивно"}
            </Badge>
          </div>
        </CardHeader>

        <CardContent className="relative px-6 pb-5">
          <div className="space-y-4">
            {/* Identifier */}
            <div className="space-y-2">
              <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                Идентификатор
              </p>
              <div className="bg-white/60 rounded-lg p-3 border border-slate-200/50">
                <p className="text-sm text-slate-700 leading-relaxed font-medium">
                  {department.identifier}
                </p>
              </div>
            </div>

            {/* Path */}
            {department.path && (
              <div className="space-y-2">
                <p className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
                  Путь
                </p>
                <div className="bg-white/60 rounded-lg p-3 border border-slate-200/50">
                  <DepartmentPath path={department.path} />
                </div>
              </div>
            )}

            {/* Action buttons */}
            {department.isActive && (onEdit || onDelete) ? (
              <div className="flex gap-2 pt-2 justify-end flex-wrap">
                {onEdit && (
                  <Button
                    onClick={handleEdit}
                    variant="ghost"
                    size="sm"
                    className="h-9 px-3 text-slate-600 hover:text-emerald-600 hover:bg-emerald-50 transition-colors font-medium"
                  >
                    <Pencil className="h-4 w-4 mr-2" />
                    Редактировать
                  </Button>
                )}
                {onDelete && (
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
            ) : (
              <div className="h-11" /> /* Заглушка для поддержания высоты */
            )}
          </div>
        </CardContent>
      </Card>
    </Link>
  );
}
