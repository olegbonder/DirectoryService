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
import { Spinner } from "@/shared/components/ui/spinner";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import DepartmentTreeChildNode from "./department-tree-child-node";
import { useDepartmentRootsQuery } from "./model/use-department-roots-query";

export default function DepartmentTree() {
  const { rootDepartments, isPending, error, isError } =
    useDepartmentRootsQuery({
      pageSize: 100,
      page: 1,
    });

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

  if (!isPending && (!rootDepartments || rootDepartments.length === 0)) {
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
        <TreeProvider multiSelect={false}>
          <TooltipProvider>
            <TreeView>
              {rootDepartments.map((department) => (
                <TreeNode
                  key={department.id}
                  nodeId={department.id}
                  className={department.isActive ? "" : "text-red-500"}
                >
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <TreeNodeTrigger>
                        <TreeExpander
                          hasChildren={!!department.children?.length}
                        />
                        <TreeIcon hasChildren={!!department.children?.length} />
                        <TreeLabel>{department.name}</TreeLabel>
                      </TreeNodeTrigger>
                    </TooltipTrigger>
                    <TooltipContent>
                      <p>Путь: {department.path}</p>
                    </TooltipContent>
                  </Tooltip>
                  <TreeNodeContent hasChildren={!!department.children?.length}>
                    <DepartmentTreeChildNode
                      parentId={department.id}
                      level={1}
                    />
                  </TreeNodeContent>
                </TreeNode>
              ))}
            </TreeView>
          </TooltipProvider>
        </TreeProvider>
      </>
    );
  }
}
