"use client";

import { ReactNode, useState, cloneElement } from "react";
import { Button } from "@/shared/components/ui/button";
import { Building2, List } from "lucide-react";
import DepartmentTree from "@/features/departments/department-tree";
import DepartmentList from "@/features/departments/departments-list";
import CreateDepartmentDialog from "@/features/departments/create-department-dialog";
import {
  DepartmentsViewMode,
  setDepartmentsViewMode,
  useDepartmentsViewMode,
} from "./model/departments-show-mode-store";

export default function DepartmentDisplayMode() {
  const { viewMode } = useDepartmentsViewMode();
  const [createOpen, setCreateOpen] = useState(false);
  type departmentViewModeButtons = {
    value: DepartmentsViewMode;
    label: string;
    icon: ReactNode;
  };

  const viewModeButtons: departmentViewModeButtons[] = [
    { value: "tree", label: "Дерево", icon: <Building2 /> },
    { value: "list", label: "Список", icon: <List /> },
  ];

  return (
    <div className="container mx-auto py-8">
      <div className="mb-6">
        <div className="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-4">
          <div>
            <h1 className="text-4xl font-bold bg-linear-to-r from-emerald-600 to-cyan-600 bg-clip-text text-transparent mb-2">
              Подразделения
            </h1>
            <p className="text-slate-600">
              Управление подразделениями в организации
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <Button
              onClick={() => setCreateOpen(true)}
              className="bg-linear-to-r from-emerald-600 to-emerald-700 hover:from-emerald-700 hover:to-emerald-800 text-white font-semibold px-6 py-3 h-auto rounded-lg shadow-lg hover:shadow-xl transition-all duration-300 transform hover:scale-105"
            >
              <span className="mr-2 text-lg">+</span>
              Создать подразделение
            </Button>
            <div className="flex rounded-lg border border-slate-200 bg-slate-50 p-1 items-center">
              {viewModeButtons.map((button) => (
                <Button
                  key={button.value}
                  variant="ghost"
                  onClick={() => setDepartmentsViewMode(button.value)}
                  className={`rounded-md px-4 py-2 text-sm font-medium transition-colors h-9 ${
                    viewMode === button.value
                      ? "bg-white text-blue-600 shadow-sm"
                      : "text-slate-600 hover:bg-slate-100"
                  }`}
                >
                  {button.icon || <span className="h-4 w-4 mr-2" />}
                  {button.label}
                </Button>
              ))}
            </div>
          </div>
        </div>
      </div>
      {viewMode === "tree" ? <DepartmentTree /> : <DepartmentList />}
      <CreateDepartmentDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  );
}
