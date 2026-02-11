import { useMemo } from "react";
import { useDepartmentChildren } from "./model/use-department-children";
import { type TreeNode } from "./model/use-expanded-nodes";
import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  ChevronDown,
  ChevronRight,
  File,
  Folder,
  FolderOpen,
} from "lucide-react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";

type RecursiveComponentType = React.ComponentType<{
  node: TreeNode;
  level: number;
  ChildComponent: RecursiveComponentType;
}>;

interface ChildDepartmentNodeProps {
  node: TreeNode;
  level: number;
  isExpanded: boolean;
  isLoading: boolean;
  hasMoreChildren: boolean;
  onToggle: (nodeId: string, hasMoreChildren: boolean) => void;
  onLoadMore: (parentId: string) => void;
  ChildComponent: RecursiveComponentType; // Компонент для отображения дочерних узлов
}

export const ChildDepartmentNode = ({
  node,
  level,
  isExpanded,
  isLoading,
  hasMoreChildren,
  onToggle,
  onLoadMore,
  ChildComponent,
}: ChildDepartmentNodeProps) => {
  const { childDepartments, isPending, error } = useDepartmentChildren(node.id);

  const hasChildren = useMemo(() => {
    return hasMoreChildren || (childDepartments && childDepartments.length > 0);
  }, [hasMoreChildren, childDepartments]);

  return (
    <div key={node.id} className="select-none">
      <div
        className={`flex items-center py-1 px-2 hover:bg-gray-100 rounded cursor-pointer ${
          node.isActive ? "" : "text-red-500"
        }`}
        style={{ paddingLeft: `${level * 20 + 8}px` }}
        onClick={() => onToggle(node.id, hasMoreChildren)}
      >
        {hasChildren ? (
          isExpanded ? (
            <ChevronDown className="h-4 w-4 mr-1" />
          ) : (
            <ChevronRight className="h-4 w-4 mr-1" />
          )
        ) : (
          <span className="w-5 h-4 mr-1"></span>
        )}

        {hasChildren ? (
          isExpanded ? (
            <FolderOpen className="h-4 w-4 mr-2" />
          ) : (
            <Folder className="h-4 w-4 mr-2" />
          )
        ) : (
          <File className="h-4 w-4 mr-2" />
        )}

        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <span className="flex-1 truncate">
                {node.name} {!node.isActive && "(неактивно)"}
              </span>
            </TooltipTrigger>
            <TooltipContent>
              <p>Путь: {node.path}</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>

        {isLoading && <Spinner className="h-4 w-4 ml-2" />}
      </div>

      {isExpanded && (
        <div>
          {isPending && !childDepartments?.length && (
            <div style={{ paddingLeft: `${(level + 1) * 20 + 8}px` }}>
              {[...Array(3)].map((_, idx) => (
                <div
                  key={`loading-${node.id}-${idx}`}
                  className="flex items-center py-2 px-2"
                >
                  <Skeleton className="w-4 h-4 mr-2" />
                  <Skeleton className="h-4 flex-1" />
                </div>
              ))}
            </div>
          )}

          {error && (
            <div
              style={{ paddingLeft: `${(level + 1) * 20 + 8}px` }}
              className="text-red-500 text-sm p-2"
            >
              Ошибка загрузки дочерних элементов
            </div>
          )}

          {childDepartments && childDepartments.length > 0 && (
            <>
              {childDepartments.map((child) => (
                <ChildComponent
                  key={child.id}
                  node={child}
                  level={level + 1}
                  ChildComponent={ChildComponent}
                />
              ))}

              {/* Кнопка "Показать ещё" может быть добавлена отдельно, если нужна бесконечная пагинация */}
            </>
          )}

          {hasMoreChildren && !isPending && (
            <div style={{ paddingLeft: `${(level + 1) * 20 + 8}px` }}>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onLoadMore(node.id)}
                className="text-xs h-6 px-2"
              >
                Показать ещё
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
