import { useState, useCallback } from "react";

export interface TreeNode {
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
}

export interface LoadedNodesState {
  [parentId: string]: TreeNode[];
}

export function useExpandedNodes() {
  const [expandedNodes, setExpandedNodes] = useState<Record<string, boolean>>(
    () => {
      // Попытка загрузить состояние из localStorage
      if (typeof window !== "undefined") {
        try {
          const saved = localStorage.getItem("departmentTreeExpandedNodes");
          return saved ? JSON.parse(saved) : {};
        } catch (e) {
          console.warn("Could not parse expanded nodes from localStorage", e);
          return {};
        }
      }
      return {};
    },
  );

  const toggleNode = useCallback((nodeId: string) => {
    setExpandedNodes((prev) => {
      const newExpandedNodes = {
        ...prev,
        [nodeId]: !prev[nodeId],
      };

      // Сохраняем состояние в localStorage
      try {
        localStorage.setItem(
          "departmentTreeExpandedNodes",
          JSON.stringify(newExpandedNodes),
        );
      } catch (e) {
        console.warn("Could not save expanded nodes to localStorage", e);
      }

      return newExpandedNodes;
    });
  }, []);

  // Функция для получения всех раскрытых узлов
  const getExpandedNodeIds = useCallback(() => {
    return Object.keys(expandedNodes).filter((id) => expandedNodes[id]);
  }, [expandedNodes]);

  // Функция для проверки, является ли узел раскрытым
  const isNodeExpanded = useCallback(
    (nodeId: string) => {
      return !!expandedNodes[nodeId];
    },
    [expandedNodes],
  );

  return {
    expandedNodes,
    toggleNode,
    getExpandedNodeIds,
    isNodeExpanded,
  };
}
