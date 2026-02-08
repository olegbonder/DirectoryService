import * as React from "react";
import { useDepartmentDictionary } from "./model/use-department-dictionary";
import { MultiSelect } from "@/shared/components/ui/multi-select";
import { Spinner } from "@/shared/components/ui/spinner";
import { DictionaryItemResponse, PAGE_SIZE } from "@/shared/api/types";

type DepartmentMultiSelectProps = {
  selectedDepartmentIds?: string[];
  excludeDepartmentIds?: string[];
  onDepartmentChange: (departmentIds: string[]) => void;
};

export default function DepartmentMultiSelect({
  selectedDepartmentIds,
  excludeDepartmentIds,
  onDepartmentChange,
}: DepartmentMultiSelectProps) {
  const {
    departments: selectedDepartments,
    isPending: isSelectedPending,
    isError: isSelectedError,
  } = useDepartmentDictionary({
    pageSize: PAGE_SIZE,
    departmentIds:
      selectedDepartmentIds && selectedDepartmentIds.length > 0
        ? selectedDepartmentIds
        : undefined,
    showOnlyParents: false,
  });

  // Загружаем все департаменты постранично
  const {
    departments: allDepartments,
    isPending: isAllPending,
    isError: isAllError,
    error: allError,
    isFetchingNextPage: isAllFetchingNextPage,
    hasNextPage: isAllHasNextPage,
    fetchNextPage: allFetchNextPage,
  } = useDepartmentDictionary({
    pageSize: PAGE_SIZE,
    showOnlyParents: false,
  });

  // Объединяем загруженные департаменты
  const combinedDepartments = React.useMemo(() => {
    const allDeptMap = new Map<string, boolean>();
    const result: DictionaryItemResponse[] = [];

    // Добавляем сначала выбранные департаменты, чтобы они были доступны
    if (selectedDepartments) {
      selectedDepartments.forEach((dept) => {
        if (!allDeptMap.has(dept.id)) {
          result.push(dept);
          allDeptMap.set(dept.id, true);
        }
      });
    }

    // Затем добавляем остальные департаменты
    if (allDepartments) {
      allDepartments.forEach((dept) => {
        if (!allDeptMap.has(dept.id)) {
          result.push(dept);
          allDeptMap.set(dept.id, true);
        }
      });
    }

    // Фильтруем результат по excludeDepartmentIds
    let filteredResult = result;
    if (excludeDepartmentIds && excludeDepartmentIds.length > 0) {
      filteredResult = result.filter(
        (dept) => !excludeDepartmentIds.includes(dept.id),
      );
    }

    return filteredResult;
  }, [selectedDepartments, allDepartments, excludeDepartmentIds]);

  const multiSelectOptions = React.useMemo(() => {
    return combinedDepartments.map((dept) => ({
      value: dept.id,
      label: dept.name,
    }));
  }, [combinedDepartments]);

  // Фильтруем defaultValue, чтобы он содержал только те ID, которые представлены в options
  const filteredDefaultValues = React.useMemo(() => {
    if (!selectedDepartmentIds || !selectedDepartmentIds.length) return [];
    const availableIds = new Set(
      multiSelectOptions.map((option) => option.value),
    );
    return selectedDepartmentIds.filter((id) => availableIds.has(id));
  }, [selectedDepartmentIds, multiSelectOptions]);

  // Определяем, показывать ли компонент
  const showComponent =
    !isAllPending &&
    !isAllError &&
    !isSelectedPending &&
    !isSelectedError &&
    combinedDepartments &&
    combinedDepartments.length > 0;

  return (
    <>
      {isAllPending && <Spinner />}
      {!isAllPending && allError && (
        <p className="text-red-500">
          Ошибка загрузки подразделений: {allError?.message}
        </p>
      )}
      {showComponent && (
        <MultiSelect
          className="flex-1"
          options={multiSelectOptions}
          onValueChange={onDepartmentChange}
          defaultValue={filteredDefaultValues}
          placeholder="Выберите подразделение"
          searchPlaceholder="Поиск..."
          morePlaceholder="ещё"
          notFoundPlaceholder="Ничего не найдено"
          disabled={isAllFetchingNextPage}
          loadMore={allFetchNextPage}
          hasNextPage={isAllHasNextPage}
          isLoadingMore={isAllFetchingNextPage}
          maxDisplayCount={3}
        />
      )}
      {!isAllPending &&
        !isAllError &&
        !isSelectedPending &&
        !isSelectedError &&
        combinedDepartments &&
        combinedDepartments.length === 0 && (
          <p className="text-muted-foreground">
            Все доступные подразделения уже добавлены.
          </p>
        )}
    </>
  );
}
