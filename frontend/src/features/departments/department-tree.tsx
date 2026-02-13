"use client";

import {
  TreeExpander,
  TreeIcon,
  TreeLabel,
  TreeNode,
  TreeNodeContent,
  TreeNodeTrigger,
  TreeProvider,
  TreeView,
} from "@/shared/components/ui/tree";
import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import DepartmentTreeChildNode from "./department-tree-child-node";
import {
  setDepartmentsTreeExpandedNodes,
  useDepartmentsExpandedNodes,
} from "./model/departments-tree-store";
import { useDepartmentRoots } from "./model/use-department-roots";

export default function DepartmentTree() {
  const { expandedNodes } = useDepartmentsExpandedNodes();
  const {
    rootDepartments,
    isPending,
    isError,
    error,
    hasNextPage,
    isFetchingNextPage,
    fetchNextPage,
  } = useDepartmentRoots();

  if (isPending) {
    return (
      <div className="flex justify-center items-center py-4">
        <Spinner />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-800">
        <p className="font-semibold">Ошибка загрузки корневых подразделений</p>
        <p className="text-sm mt-1">{error?.message}</p>
      </div>
    );
  }

  if (!rootDepartments || rootDepartments.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <div className="text-slate-400 text-center">
          <p className="text-lg font-medium mb-2">
            Корневые подразделения не найдены
          </p>
        </div>
      </div>
    );
  }

  if (!isPending && rootDepartments && rootDepartments.length > 0) {
    return (
      <>
        <TreeProvider
          multiSelect={false}
          defaultExpandedIds={expandedNodes}
          onExpandChange={setDepartmentsTreeExpandedNodes}
        >
          <TooltipProvider>
            <TreeView>
              {rootDepartments.map((department, index) => {
                const isLast =
                  index === rootDepartments.length - 1 && !isFetchingNextPage;
                return (
                  <TreeNode
                    key={department.id}
                    nodeId={department.id}
                    className={department.isActive ? "" : "text-red-500"}
                    isLast={isLast}
                  >
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <TreeNodeTrigger>
                          <TreeExpander
                            hasChildren={!!department.children?.length}
                          />
                          <TreeIcon
                            hasChildren={!!department.children?.length}
                          />
                          <TreeLabel>{department.name}</TreeLabel>
                        </TreeNodeTrigger>
                      </TooltipTrigger>
                      <TooltipContent>
                        <p>Путь: {department.path}</p>
                      </TooltipContent>
                    </Tooltip>
                    <TreeNodeContent
                      hasChildren={!!department.children?.length}
                    >
                      <DepartmentTreeChildNode
                        parentId={department.id}
                        level={1}
                      />
                    </TreeNodeContent>
                  </TreeNode>
                );
              })}
              {hasNextPage && (
                <div className="py-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => fetchNextPage()}
                    disabled={isFetchingNextPage}
                  >
                    {isFetchingNextPage ? "Загрузка..." : "Показать ещё"}
                  </Button>
                </div>
              )}
            </TreeView>
          </TooltipProvider>
        </TreeProvider>
      </>
    );
  }
}
