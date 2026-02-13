import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  TreeExpander,
  TreeIcon,
  TreeLabel,
  TreeNode,
  TreeNodeContent,
  TreeNodeTrigger,
} from "@/shared/components/ui/tree";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import { useDepartmentChildrenQuery } from "./model/use-department-children-query";

type DepartmentTreeChildNodeProps = {
  parentId: string;
  level: number;
};

export default function DepartmentTreeChildNode({
  parentId,
  level,
}: DepartmentTreeChildNodeProps) {
  const { childDepartments, isPending, isError, error } =
    useDepartmentChildrenQuery(parentId, 1);

  if (isPending) {
    return (
      <div className="pl-6">
        {[...Array(3)].map((_, idx) => (
          <div
            key={`loading-${parentId}-${idx}`}
            className="flex items-center py-2 px-2"
          >
            <Skeleton className="w-4 h-4 mr-2" />
            <Skeleton className="h-4 flex-1" />
          </div>
        ))}
      </div>
    );
  }

  if (isError) {
    return (
      <div className="pl-6 bg-red-50 border-l-4 border-red-500 p-4 rounded text-red-800">
        <p className="font-semibold">Ошибка загрузки дочерних подразделений</p>
        <p className="text-sm mt-1">{error?.message}</p>
      </div>
    );
  }

  if (!childDepartments || childDepartments.length === 0) {
    return null;
  }

  return (
    <>
      {childDepartments.map((department, index) => {
        const isLast = index == childDepartments.length - 1;
        return (
          <TreeNode
            key={department.id}
            nodeId={department.id}
            level={level}
            isLast={isLast}
          >
            <Tooltip>
              <TooltipTrigger asChild>
                <TreeNodeTrigger
                  className={department.isActive ? "" : "text-red-500"}
                >
                  <TreeExpander hasChildren={!!department.hasMoreChildren} />
                  <TreeIcon hasChildren={!!department.hasMoreChildren} />
                  <TreeLabel
                    className={department.isActive ? "" : "text-red-500"}
                  >
                    {department.name} {!department.isActive && "(неактивно)"}
                  </TreeLabel>
                </TreeNodeTrigger>
              </TooltipTrigger>
              <TooltipContent>
                <p>Путь: {department.path}</p>
              </TooltipContent>
            </Tooltip>

            <TreeNodeContent hasChildren>
              <DepartmentTreeChildNode
                parentId={department.id}
                level={level + 1}
              />
            </TreeNodeContent>
          </TreeNode>
        );
      })}
    </>
  );
}
