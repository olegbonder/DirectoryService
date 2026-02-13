"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { EnvelopeError } from "@/shared/api/errors";
import { useGetGlobalFilter } from "@/shared/stores/global-search-store";
import { useGetDepartmentsFilter } from "./model/departments-filters-store";
import { useDepartmentsList } from "./model/use-departments-list";
import { PAGE_SIZE } from "@/shared/api/types";
import DepartmentFilters from "./departments-filters";
import DepartmentSort from "./departments-sort";
import DepartmentCard from "./department-card";
import { Department } from "@/entities/departments/types";
import DeleteDepartmentAlertDialog from "./delete-department-alert";

export default function DepartmentList() {
  const globalSearch = useGetGlobalFilter();
  const {
    name,
    identifier,
    parentId,
    locationIds,
    isActive,
    orderBy,
    orderDirection,
  } = useGetDepartmentsFilter();

  const {
    departments,
    isPending,
    error,
    isError,
    isFetchingNextPage,
    cursorRef,
  } = useDepartmentsList({
    name: name === "" ? globalSearch : name,
    identifier,
    parentId,
    locationIds,
    isActive,
    pageSize: PAGE_SIZE,
    orderBy,
    orderDirection,
  });

  const [deleteOpen, setDeleteOpen] = useState(false);
  const [selectedDepartment, setSelectedDepartment] = useState<
    Department | undefined
  >(undefined);

  const getError = (error: Error): string => {
    return error instanceof EnvelopeError
      ? error.getAllMessages()
      : error.message;
  };

  return (
    <div className="container">
      {isError && (
        <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-800">
          <p className="font-semibold">Ошибка загрузки</p>
          <p className="text-sm mt-1">{getError(error!)}</p>
        </div>
      )}

      {!isError && (
        <div className="mb-6 space-y-4">
          <div className="p-4 bg-white rounded-lg shadow-sm border border-slate-200">
            <DepartmentFilters />
          </div>
          <div className="p-4 bg-white rounded-lg shadow-sm border border-slate-200">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold text-slate-700">
                Сортировка
              </h3>
              <div className="flex items-center gap-4">
                <DepartmentSort />
              </div>
            </div>
          </div>
        </div>
      )}

      {isPending && (
        <div className="flex justify-center items-center py-16">
          <div className="text-center">
            <Spinner />
            <p className="text-slate-400 mt-4">Загрузка подразделений...</p>
          </div>
        </div>
      )}

      {!isPending && (!departments || departments.length === 0) && (
        <div className="flex flex-col items-center justify-center py-20">
          <div className="text-slate-400 text-center">
            <p className="text-lg font-medium mb-2">Подразделения не найдены</p>
          </div>
        </div>
      )}

      {!isPending && departments && departments.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12">
          {departments.map((department) => (
            <DepartmentCard
              key={department.id}
              department={department}
              onEdit={() => {
                setSelectedDepartment(department);
              }}
              onDelete={() => {
                setSelectedDepartment(department);
                setDeleteOpen(true);
              }}
            />
          ))}
        </div>
      )}
      {selectedDepartment && (
        <div>
          <DeleteDepartmentAlertDialog
            department={selectedDepartment}
            open={deleteOpen}
            onOpenChange={setDeleteOpen}
            onConfirm={() => {
              setDeleteOpen(false);
              setSelectedDepartment(undefined);
            }}
          />
        </div>
      )}

      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
