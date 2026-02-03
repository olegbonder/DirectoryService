"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { EnvelopeError } from "@/shared/api/errors";
import { Button } from "@/shared/components/ui/button";
import { useGetGlobalFilter } from "@/shared/stores/global-search-store";
import {
  setPositionsPage,
  useGetPositionsFilter,
} from "./model/positions-filters-store";
import { usePositionsList } from "./model/use-positions-list";
import PositionCard from "./position-card";
import ItemsPagination from "../pagination/pagination";
import CreatePositionDialog from "./create-position-dialog";
import PositionFilters from "./positions-filters";
import DeletePositionAlertDialog from "./delete-position-alert";
import { Position } from "@/entities/positions/types";

export default function PositionList() {
  const globalSearch = useGetGlobalFilter();
  const { search, departmentIds, isActive, pageSize, page } =
    useGetPositionsFilter();

  const { positions, isPending, error, isError, totalPages } = usePositionsList(
    {
      departmentIds,
      search: search === "" ? globalSearch : search,
      isActive,
      pageSize,
      page: departmentIds.length === 0 ? page : 1,
    },
  );

  const [createOpen, setCreateOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [selectedPosition, setSelectedPosition] = useState<
    Position | undefined
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
            <h1 className="text-4xl font-bold bg-linear-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-2">
              Позиции
            </h1>
            <p className="text-slate-600">
              Управление должностями в организации
            </p>
          </div>
          {!isError && (
            <Button
              onClick={() => setCreateOpen(true)}
              className="bg-linear-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white font-semibold px-6 py-3 h-auto rounded-lg shadow-lg hover:shadow-xl transition-all duration-300 transform hover:scale-105"
            >
              <span className="mr-2 text-lg">+</span>
              Создать позицию
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
          <PositionFilters />
        </div>
      )}
      {isPending && (
        <div className="flex justify-center items-center py-16">
          <div className="text-center">
            <Spinner />
            <p className="text-slate-400 mt-4">Загрузка позиций...</p>
          </div>
        </div>
      )}

      {!isPending && (!positions || positions.length === 0) && (
        <div className="flex flex-col items-center justify-center py-20">
          <div className="text-slate-400 text-center">
            <p className="text-lg font-medium mb-2">Позиции не найдены</p>
          </div>
        </div>
      )}

      {!isPending && positions && positions.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 mb-12">
          {positions.map((position) => (
            <PositionCard
              key={position.id}
              position={position}
              onDelete={() => {
                setSelectedPosition(position);
                setDeleteOpen(true);
              }}
            />
          ))}
        </div>
      )}

      {!isPending &&
        departmentIds.length === 0 &&
        positions &&
        positions.length > 0 &&
        totalPages > 1 && (
          <div className="flex justify-center mt-12">
            <div className="bg-white rounded-lg shadow-md p-6 border border-slate-200">
              <ItemsPagination
                totalPages={totalPages}
                page={page}
                setPage={setPositionsPage}
              />
            </div>
          </div>
        )}

      <CreatePositionDialog open={createOpen} onOpenChange={setCreateOpen} />
      {selectedPosition && (
        <DeletePositionAlertDialog
          position={selectedPosition}
          open={deleteOpen}
          onOpenChange={setDeleteOpen}
          onConfirm={() => {
            // Handle delete action here
            setDeleteOpen(false);
            setSelectedPosition(undefined);
          }}
        />
      )}
    </div>
  );
}
