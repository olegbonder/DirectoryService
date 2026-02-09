import { useState, useCallback, useEffect } from "react";
import { useDepartmentRoots } from "./model/use-department-roots";
import { departmentsApi } from "@/entities/departments/api";
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
import { useQueryClient } from "@tanstack/react-query";

type TreeNode = {
  id: string;
  name: string;
  depth: number;
  path: string;
  isActive: boolean;
  hasMoreChildren: boolean;
  children?: TreeNode[];
  isExpanded?: boolean;
  isLoading?: boolean;
  page?: number;
  hasMore?: boolean;
};

export default function DepartmentTree() {
  const [expandedNodes, setExpandedNodes] = useState<Record<string, boolean>>(
    {},
  );
  const [loadedNodes, setLoadedNodes] = useState<Record<string, TreeNode[]>>(
    {},
  );
  const [loadingNodes, setLoadingNodes] = useState<Set<string>>(new Set());
  const [loadingChildrenNodes, setLoadingChildrenNodes] = useState<Set<string>>(
    new Set(),
  );
  const [currentPage, setCurrentPage] = useState<Record<string, number>>({});
  const queryClient = useQueryClient();

  const { rootDepartments, isPending, error, isError } = useDepartmentRoots();

  const loadChildren = useCallback(
    async (parentId: string, page: number = 1) => {
      if (loadingNodes.has(parentId)) return;

      setLoadingNodes((prev) => new Set(prev).add(parentId));
      setLoadingChildrenNodes((prev) => new Set(prev).add(parentId));

      try {
        const response = await queryClient.fetchQuery({
          queryKey: ["department-children", parentId, page],
          queryFn: () =>
            departmentsApi.getChildDepartments({
              parentId,
              page,
              pageSize: 10,
            }),
          staleTime: 5 * 60 * 1000,
        });

        if (response) {
          setLoadedNodes((prev) => ({
            ...prev,
            [parentId]: [...(prev[parentId] || []), ...(response.items || [])],
          }));

          setCurrentPage((prev) => ({
            ...prev,
            [parentId]: page,
          }));
        }
      } catch (err) {
        console.error("Error loading children:", err);
      } finally {
        setLoadingNodes((prev) => {
          const newSet = new Set(prev);
          newSet.delete(parentId);
          return newSet;
        });
        setLoadingChildrenNodes((prev) => {
          const newSet = new Set(prev);
          newSet.delete(parentId);
          return newSet;
        });
      }
    },
    [loadingNodes, queryClient],
  );

  const toggleNode = useCallback(
    async (nodeId: string, hasMoreChildren: boolean) => {
      if (!hasMoreChildren) return;

      const isCurrentlyExpanded = expandedNodes[nodeId];

      // Переключаем состояние раскрытия
      setExpandedNodes((prev) => ({
        ...prev,
        [nodeId]: !isCurrentlyExpanded,
      }));

      // Если узел раскрывается впервые и у него есть дети, загружаем их
      if (!isCurrentlyExpanded && !loadedNodes[nodeId]) {
        await loadChildren(nodeId, 1);
      }
    },
    [expandedNodes, loadedNodes, loadChildren],
  );

  const loadMoreChildren = useCallback(
    async (parentId: string) => {
      const nextPage = (currentPage[parentId] || 1) + 1;
      await loadChildren(parentId, nextPage);
    },
    [currentPage, loadChildren],
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
    const hasLoadedChildren =
      loadedNodes[node.id] && loadedNodes[node.id].length > 0;
    const isExpanded = expandedNodes[node.id] || false;
    const isLoading = loadingNodes.has(node.id);
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
          {hasMoreChildren || hasLoadedChildren ? (
            isExpanded ? (
              <ChevronDown className="h-4 w-4 mr-1" />
            ) : (
              <ChevronRight className="h-4 w-4 mr-1" />
            )
          ) : (
            <span className="w-5 h-4 mr-1"></span>
          )}

          {hasMoreChildren || hasLoadedChildren ? (
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
            {loadingChildrenNodes.has(node.id) &&
              !loadedNodes[node.id]?.length && (
                <div style={{ paddingLeft: `${(level + 1) * 20 + 8}px` }}>
                  {[...Array(3)].map((_, idx) => (
                    <div key={idx} className="flex items-center py-2 px-2">
                      <Skeleton className="w-4 h-4 mr-2" />
                      <Skeleton className="h-4 flex-1" />
                    </div>
                  ))}
                </div>
              )}

            {hasLoadedChildren &&
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
                  disabled={loadingNodes.has(node.id)}
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
