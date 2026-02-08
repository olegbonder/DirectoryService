"use client";

import { useState } from "react";
import useDepartment from "./model/use-department";
import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import { ArrowLeft, Pencil, MapPin, Briefcase } from "lucide-react";
import Link from "next/link";
import UpdateDepartmentDialog from "@/features/departments/update-department-dialog";
import ManageDepartmentLocationsDialog from "@/features/departments/manage-department-locations-dialog";
import PositionCard from "@/features/positions/position-card";
import DepartmentPath from "./department-path";

type DepartmentDetailProps = {
  id: string;
};
export default function DepartmentDetail({ id }: DepartmentDetailProps) {
  const [updateDepartmentOpen, setUpdateDepartmentOpen] = useState(false);
  const [manageLocationsOpen, setManageLocationsOpen] = useState(false);

  const { department, isPending, isError, error } = useDepartment(id);

  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setUpdateDepartmentOpen(true);
  };

  const handleManageLocations = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setManageLocationsOpen(true);
  };

  if (isPending) {
    return (
      <div className="flex justify-center items-center py-16">
        <div className="text-center">
          <Spinner />
          <p className="text-slate-400 mt-4">Загрузка подразделения...</p>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Link
          href="/departments"
          className="mb-4 inline-flex items-center gap-2 text-blue-600 hover:text-blue-700"
        >
          <ArrowLeft className="h-4 w-4" />
          Вернуться к подразделениям
        </Link>
        <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-800">
          <p className="font-semibold">Ошибка загрузки подразделения</p>
          <p className="text-sm mt-1">{error?.message}</p>
        </div>
      </div>
    );
  }

  if (!department) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Link
          href="/departments"
          className="mb-4 inline-flex items-center gap-2 text-blue-600 hover:text-blue-700"
        >
          <ArrowLeft className="h-4 w-4" />
          Вернуться к подразделениям
        </Link>
        <div className="text-center py-16">
          <p className="text-slate-400">Подразделение не найдено</p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="container mx-auto px-4 py-8">
        <Link
          href="/departments"
          className="mb-4 inline-flex items-center gap-2 text-blue-600 hover:text-blue-700"
        >
          <ArrowLeft className="h-4 w-4" />
          Вернуться к подразделениям
        </Link>

        <div className="bg-white rounded-lg shadow-md p-8 mb-6">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h1 className="text-4xl font-bold text-slate-900">
                {department.name}
              </h1>
            </div>
            {department.isActive && (
              <div className="flex gap-2">
                <Button
                  onClick={handleEdit}
                  className="bg-blue-600 hover:bg-blue-700 text-white"
                >
                  <Pencil className="h-4 w-4 mr-2" />
                  Редактировать
                </Button>
              </div>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div className="bg-slate-50 rounded-lg p-4">
              <p className="text-slate-600 text-sm">Идентификатор</p>
              <p className="text-lg font-semibold mt-1">
                {department.identifier}
              </p>
            </div>
            <div className="bg-slate-50 rounded-lg p-4">
              <p className="text-slate-600 text-sm">Глубина в иерархии</p>
              <p className="text-lg font-semibold mt-1">{department.depth}</p>
            </div>
            <div className="bg-slate-50 rounded-lg p-4">
              <p className="text-slate-600 text-sm">
                Родительское подразделение
              </p>
              <p className="text-lg font-semibold mt-1">
                {department.parentId ? "Установлено" : "Отсутствует"}
              </p>
            </div>
            <div className="bg-slate-50 rounded-lg p-4">
              <p className="text-slate-600 text-sm">Статус</p>
              <p className="text-lg font-semibold mt-1">
                {department.isActive ? (
                  <span className="text-green-600">Активное</span>
                ) : (
                  <span className="text-red-600">Неактивное</span>
                )}
              </p>
            </div>
            <div className="bg-slate-50 rounded-lg p-4 md:col-span-2">
              <p className="text-slate-600 text-sm">Путь в иерархии</p>
              <div className="text-lg font-semibold mt-1">
                <DepartmentPath
                  path={department.path}
                  itemClassName="text-lg font-semibold text-slate-900"
                />
              </div>
            </div>
          </div>
        </div>

        {/* Card with locations list */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-slate-900 flex items-center gap-2">
              <MapPin className="h-5 w-5 text-green-600" />
              Локации подразделения
            </h2>
            {department.isActive && (
              <Button
                onClick={handleManageLocations}
                className="bg-green-600 hover:bg-green-700 text-white"
              >
                <MapPin className="h-4 w-4 mr-2" />
                Управление локациями
              </Button>
            )}
          </div>

          {department.locations && department.locations.length > 0 ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
              {department.locations.map((location) => (
                <div
                  key={location.id}
                  className="border border-slate-200 rounded-lg p-3 bg-slate-50 flex items-center"
                >
                  <MapPin className="h-4 w-4 text-green-500 mr-2" />
                  <span className="text-slate-700">{location.name}</span>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-4 text-slate-500">
              Нет привязанных локаций
            </div>
          )}
        </div>

        {/* Card with positions list */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-slate-900 flex items-center gap-2">
              <Briefcase className="h-5 w-5 text-blue-600" />
              Позиции подразделения
            </h2>
          </div>
          {department.positions && department.positions.length > 0 ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
              {department.positions.map((position) => (
                <div
                  key={position}
                  className="border border-slate-200 rounded-lg p-3 bg-slate-50 flex items-center"
                >
                  <Briefcase className="h-4 w-4 text-blue-500 mr-2" />
                  <span className="text-slate-700">{position}</span>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-4 text-slate-500">
              Нет назначенных позиций
            </div>
          )}
        </div>
      </div>

      {department && (
        <>
          <UpdateDepartmentDialog
            department={department}
            open={updateDepartmentOpen}
            onOpenChange={setUpdateDepartmentOpen}
          />
          <ManageDepartmentLocationsDialog
            department={department}
            open={manageLocationsOpen}
            onOpenChange={setManageLocationsOpen}
          />
        </>
      )}
    </>
  );
}
