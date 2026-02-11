import { useState, useCallback, useEffect } from "react";
import { useDepartmentRoots } from "./model/use-department-roots";
import { Spinner } from "@/shared/components/ui/spinner";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  File,
  Folder,
  FolderOpen,
  ChevronDown,
  ChevronRight,
} from "lucide-react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import { useExpandedNodes, type TreeNode } from "./model/use-expanded-nodes";
import { useTreeData } from "./model/use-tree-data";

export default function DepartmentTree() {
  const {
    expandedNodes,
    toggleNode: toggleNodeFromHook,
    getExpandedNodeIds,
    isNodeExpanded,
  } = useExpandedNodes();

  const {
    loadedNodes,
    loadChildren,
    loadMoreChildren,
    hasLoadedChildren,
    isNodeLoading,
    isChildrenLoading,
  } = useTreeData();

  const { rootDepartments, isPending, error, isError } = useDepartmentRoots();

  // Подгрузка детей для раскрытых узлов
  useEffect(() => {
    const loadExpandedNodesChildren = async () => {
      if (!rootDepartments) return;

      // Загружаем детей для корневых раскрытых узлов
      for (const dept of rootDepartments) {
        if (isNodeExpanded(dept.id) && !loadedNodes[dept.id]) {
          await loadChildren(dept.id, 1);
        }
      }

      // Получаем все раскрытые узлы
      const expandedNodeIds = getExpandedNodeIds();

      // Загружаем детей для вложенных раскрытых узлов
      for (const nodeId of expandedNodeIds) {
        if (!loadedNodes[nodeId]) {
          // Проверяем, является ли этот узел дочерним для какого-либо уже загруженного узла
          let isChildOfLoadedNode = false;

          for (const parentId in loadedNodes) {
            if (loadedNodes[parentId]?.some((child) => child.id === nodeId)) {
              isChildOfLoadedNode = true;
              break;
            }
          }

          // Если узел является дочерним для загруженного узла, но сам не загружен - загружаем его
          if (isChildOfLoadedNode) {
            await loadChildren(nodeId, 1);
          }
        }
      }
    };

    loadExpandedNodesChildren();
  }, [
    expandedNodes,
    rootDepartments,
    loadedNodes,
    loadChildren,
    isNodeExpanded,
    getExpandedNodeIds,
  ]);

  const toggleNode = useCallback(
    async (nodeId: string, hasMoreChildren: boolean) => {
      if (!hasMoreChildren) return;

      const isCurrentlyExpanded = isNodeExpanded(nodeId);

      // Переключаем состояние раскрытия с помощью хука
      toggleNodeFromHook(nodeId);

      // Если узел раскрывается впервые и у него есть дети, загружаем их
      if (!isCurrentlyExpanded && !loadedNodes[nodeId]) {
        await loadChildren(nodeId, 1);
      }
    },
    [isNodeExpanded, loadedNodes, loadChildren, toggleNodeFromHook],
  );

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

  const renderTreeNode = (node: TreeNode, level = 0) => {
    const nodeHasLoadedChildren = hasLoadedChildren(node.id);
    const isExpanded = isNodeExpanded(node.id);
    const isLoading = isNodeLoading(node.id);
    const hasMoreChildren = node.hasMoreChildren;

    return (
      <div key={node.id} className="select-none">
        <div
          className={`flex items-center py-1 px-2 hover:bg-gray-100 rounded cursor-pointer ${
            node.isActive ? "" : "text-red-500"
          }`}
          style={{ paddingLeft: `${level * 20 + 8}px` }}
          onClick={() => toggleNode(node.id, hasMoreChildren)}
        >
          {hasMoreChildren || nodeHasLoadedChildren ? (
            isExpanded ? (
              <ChevronDown className="h-4 w-4 mr-1" />
            ) : (
              <ChevronRight className="h-4 w-4 mr-1" />
            )
          ) : (
            <span className="w-5 h-4 mr-1"></span>
          )}

          {hasMoreChildren || nodeHasLoadedChildren ? (
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
            {isChildrenLoading(node.id) && !loadedNodes[node.id]?.length && (
              <div style={{ paddingLeft: `${(level + 1) * 20 + 8}px` }}>
                {[...Array(3)].map((_, idx) => (
                  <div
                    key={`${node.id}-loading-${idx}`}
                    className="flex items-center py-2 px-2"
                  >
                    <Skeleton className="w-4 h-4 mr-2" />
                    <Skeleton className="h-4 flex-1" />
                  </div>
                ))}
              </div>
            )}

            {nodeHasLoadedChildren &&
              loadedNodes[node.id]?.map((child) =>
                renderTreeNode(child, level + 1),
              )}

            {hasMoreChildren && (
              <div style={{ paddingLeft: `${(level + 1) * 20 + 8}px` }}>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => loadMoreChildren(node.id)}
                  className="text-xs h-6 px-2"
                  disabled={isNodeLoading(node.id)}
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

  return !rootDepartments || rootDepartments.length === 0 ? (
    <div className="text-center py-4 text-slate-500">
      Нет корневых подразделений
    </div>
  ) : (
    <div className="flex">
      <div className="w-auto pr-4">
        <div className="overflow-y-auto bg-white">
          {rootDepartments.map((dept) =>
            renderTreeNode({
              id: dept.id,
              name: dept.name,
              depth: dept.depth,
              path: dept.path,
              isActive: dept.isActive,
              hasMoreChildren:
                dept.hasMoreChildren ||
                (dept.children && dept.children.length > 0),
            }),
          )}
        </div>
      </div>
    </div>
  );
}
