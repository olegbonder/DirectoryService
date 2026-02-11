import { useState, useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { departmentsApi, departmentsQueryOptions } from "@/entities/departments/api";
import { LoadedNodesState } from "./use-expanded-nodes";

export interface TreeDataState {
  loadedNodes: LoadedNodesState;
  loadingNodes: Set<string>;
  loadingChildrenNodes: Set<string>;
  currentPage: Record<string, number>;
}

export function useTreeData() {
  const [loadedNodes, setLoadedNodes] = useState<LoadedNodesState>({});
  const [loadingNodes, setLoadingNodes] = useState<Set<string>>(new Set());
  const [loadingChildrenNodes, setLoadingChildrenNodes] = useState<Set<string>>(
    new Set(),
  );
  const [currentPage, setCurrentPage] = useState<Record<string, number>>({});

  const queryClient = useQueryClient();

  const loadChildren = useCallback(
    async (parentId: string, page: number = 1) => {
      if (loadingNodes.has(parentId)) return;

      setLoadingNodes((prev) => new Set(prev).add(parentId));
      setLoadingChildrenNodes((prev) => new Set(prev).add(parentId));

      try {
        // Используем те же опции, что и в useDepartmentChildren, но адаптируем для fetchQuery
        const queryOptions = departmentsQueryOptions.getDepartmentChildrenInfinityOptions(parentId);
        
        // Создаем временный queryKey для конкретной страницы
        const pageQueryKey = [...queryOptions.queryKey, page];
        
        // Адаптируем queryFn для получения конкретной страницы
        const response = await queryClient.fetchQuery({
          queryKey: pageQueryKey,
          queryFn: () => departmentsApi.getChildDepartments({
            parentId,
            page,
            pageSize: 10,
          }),
          staleTime: 5 * 60 * 1000,
        });

        if (response) {
          setLoadedNodes((prev) => {
            // Создаем множество ID уже загруженных узлов для предотвращения дубликатов
            const existingIds = new Set(
              (prev[parentId] || []).map((node) => node.id),
            );
            // Фильтруем новые элементы, чтобы избежать дубликатов
            const newItems = (response.items || []).filter(
              (item) => !existingIds.has(item.id),
            );

            return {
              ...prev,
              [parentId]: [...(prev[parentId] || []), ...newItems],
            };
          });

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
    [loadingNodes, queryClient, loadedNodes],
  );

  const loadMoreChildren = useCallback(
    async (parentId: string) => {
      const nextPage = (currentPage[parentId] || 1) + 1;
      await loadChildren(parentId, nextPage);
    },
    [currentPage, loadChildren],
  );

  const hasLoadedChildren = useCallback(
    (nodeId: string) => {
      return !!(loadedNodes[nodeId] && loadedNodes[nodeId].length > 0);
    },
    [loadedNodes],
  );

  const isNodeLoading = useCallback(
    (nodeId: string) => {
      return loadingNodes.has(nodeId);
    },
    [loadingNodes],
  );

  const isChildrenLoading = useCallback(
    (nodeId: string) => {
      return loadingChildrenNodes.has(nodeId);
    },
    [loadingChildrenNodes],
  );

  return {
    loadedNodes,
    loadingNodes,
    loadingChildrenNodes,
    currentPage,
    loadChildren,
    loadMoreChildren,
    hasLoadedChildren,
    isNodeLoading,
    isChildrenLoading,
    setLoadedNodes,
  };
}
