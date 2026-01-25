"use client";

import React, { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { ArrowLeft, Pencil, Plus, Trash2 } from "lucide-react";
import Link from "next/link";
import usePosition from "@/features/positions/model/use-position";
import UpdatePositionDialog from "@/features/positions/update-position-dialog";
import AddDepartmentsToPositionDialog from "@/features/departments/add-departments-to-position-dialog";
import { DictionaryItemResponse } from "@/shared/api/types";
import DeletePositionDepartmentAlertDialog from "@/features/departments/delete-position-department-alert";

export default function PositionPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = React.use(params);
  const [updatePositionOpen, setUpdatePositionOpen] = useState(false);

  const [addDepartmentsToPositionOpen, setAddDepartmentsToPositionOpen] =
    useState(false);
  const [deletePositionDepartmentOpen, setDeletePositionDepartmentOpen] =
    useState(false);
  const [selectedDepartment, setSelectedDepartment] = useState<
    DictionaryItemResponse | undefined
  >(undefined);

  const { position, isPending, isError, error } = usePosition(id);

  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setUpdatePositionOpen(true);
  };
  if (isPending) {
    return (
      <div className="flex justify-center items-center py-16">
        <div className="text-center">
          <Spinner />
          <p className="text-slate-400 mt-4">Загрузка позиции...</p>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Link
          href="/positions"
          className="mb-4 inline-flex items-center gap-2 text-blue-600 hover:text-blue-700"
        >
          <ArrowLeft className="h-4 w-4" />
          Вернуться к позициям
        </Link>
        <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-800">
          <p className="font-semibold">Ошибка загрузки позиции</p>
          <p className="text-sm mt-1">{error?.message}</p>
        </div>
      </div>
    );
  }

  if (!position) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Link
          href="/positions"
          className="mb-4 inline-flex items-center gap-2 text-blue-600 hover:text-blue-700"
        >
          <ArrowLeft className="h-4 w-4" />
          Вернуться к позициям
        </Link>
        <div className="text-center py-16">
          <p className="text-slate-400">Позиция не найдена</p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="container mx-auto px-4 py-8">
        <Link
          href="/positions"
          className="mb-6 inline-flex items-center gap-2 text-blue-600 hover:text-blue-700"
        >
          <ArrowLeft className="h-4 w-4" />
          Вернуться к позициям
        </Link>

        <div className="bg-white rounded-lg shadow-md p-8 mb-6">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h1 className="text-4xl font-bold bg-linear-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                {position.name}
              </h1>
            </div>
            {position.isActive && (
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

          {position.description && (
            <div className="mb-6">
              <h2 className="text-lg font-semibold text-slate-900 mb-2">
                Описание
              </h2>
              <p className="text-slate-600 leading-relaxed">
                {position.description}
              </p>
            </div>
          )}

          <div className="grid grid-cols-2 gap-4 mb-6">
            <div className="bg-slate-50 rounded-lg p-4">
              <p className="text-slate-600 text-sm">Статус</p>
              <p className="text-lg font-semibold mt-1">
                {position.isActive ? (
                  <span className="text-green-600">Активная</span>
                ) : (
                  <span className="text-red-600">Неактивная</span>
                )}
              </p>
            </div>
            <div className="bg-slate-50 rounded-lg p-4">
              <p className="text-slate-600 text-sm">Создано</p>
              <p className="text-lg font-semibold mt-1">
                {new Date(position.createdAt).toLocaleDateString("ru-RU", {
                  year: "numeric",
                  month: "long",
                  day: "numeric",
                })}
              </p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-md p-8">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold text-slate-900">
              Подразделения ({position.departments.length})
            </h2>
            {position.isActive && (
              <Button
                className="bg-blue-600 hover:bg-blue-700 text-white"
                onClick={() => setAddDepartmentsToPositionOpen(true)}
              >
                <Plus className="h-4 w-4 mr-2" />
                Добавить подразделение
              </Button>
            )}
          </div>

          {position.departments.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-slate-400">Нет назначенных подразделений</p>
            </div>
          ) : (
            <div className="space-y-2">
              {position.departments.map((dept) => (
                <div
                  key={dept.id}
                  className="flex items-center justify-between p-4 bg-slate-50 rounded-lg hover:bg-slate-100 transition-colors"
                >
                  <span className="font-medium text-slate-900">
                    {dept.name}
                  </span>
                  {position.isActive && (
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-red-600 hover:text-red-700 hover:bg-red-50"
                      onClick={() => {
                        setSelectedDepartment(dept);
                        setDeletePositionDepartmentOpen(true);
                      }}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
      <div key={position.id}>
        <UpdatePositionDialog
          position={position}
          open={updatePositionOpen}
          onOpenChange={setUpdatePositionOpen}
        />
        <AddDepartmentsToPositionDialog
          positionId={position.id}
          positionDepartmentIds={position.departments.map((d) => d.id)}
          open={addDepartmentsToPositionOpen}
          onOpenChange={setAddDepartmentsToPositionOpen}
        />
        {selectedDepartment && (
          <DeletePositionDepartmentAlertDialog
            positionId={position.id}
            departmentId={selectedDepartment.id}
            departmentName={selectedDepartment.name}
            open={deletePositionDepartmentOpen}
            onOpenChange={setDeletePositionDepartmentOpen}
            onConfirm={() => {
              setSelectedDepartment(undefined);
              setDeletePositionDepartmentOpen(false);
            }}
          />
        )}
      </div>
    </>
  );
}
