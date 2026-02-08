import * as React from "react";
import { MultiSelect } from "@/shared/components/ui/multi-select";
import { Spinner } from "@/shared/components/ui/spinner";
import { DictionaryItemResponse, PAGE_SIZE } from "@/shared/api/types";
import { useLocationDictionary } from "./model/use-location-dictionary";

type LocationDictionaryProps = {
  selectedLocationIds?: string[];
  onLocationChange: (locationIds: string[]) => void;
};

export default function LocationDictionary({
  selectedLocationIds,
  onLocationChange,
}: LocationDictionaryProps) {
  const {
    locations: selectedLocations,
    isPending: isSelectedPending,
    isError: isSelectedError,
  } = useLocationDictionary({
    pageSize: PAGE_SIZE,
    locationIds:
      selectedLocationIds && selectedLocationIds.length > 0
        ? selectedLocationIds
        : undefined,
  });

  const {
    locations: allLocations,
    isPending: isAllPending,
    isError: isAllError,
    error: allError,
    isFetchingNextPage: isAllFetchingNextPage,
    hasNextPage: isAllHasNextPage,
    fetchNextPage: allFetchNextPage,
  } = useLocationDictionary({
    pageSize: PAGE_SIZE,
  });

  const combinedLocations = React.useMemo(() => {
    const allLocMap = new Map<string, boolean>();
    const result: DictionaryItemResponse[] = [];

    // Добавляем сначала выбранные локации, чтобы они были доступны
    if (selectedLocations) {
      selectedLocations.forEach((location) => {
        if (!allLocMap.has(location.id)) {
          result.push(location);
          allLocMap.set(location.id, true);
        }
      });
    }

    // Затем добавляем остальные департаменты
    if (allLocations) {
      allLocations.forEach((location) => {
        if (!allLocMap.has(location.id)) {
          result.push(location);
          allLocMap.set(location.id, true);
        }
      });
    }

    return result;
  }, [selectedLocations, allLocations]);

  const multiSelectOptions = React.useMemo(() => {
    return combinedLocations.map((location) => ({
      value: location.id,
      label: location.name,
    }));
  }, [combinedLocations]);

  // Определяем, показывать ли компонент
  const showComponent =
    !isAllPending &&
    !isAllError &&
    !isSelectedPending &&
    !isSelectedError &&
    combinedLocations &&
    combinedLocations.length > 0;

  return (
    <>
      {isAllPending && <Spinner />}
      {!isAllPending && isAllError && (
        <p className="text-red-500">
          Ошибка загрузки локаций: {allError?.message}
        </p>
      )}
      {showComponent && (
        <MultiSelect
          className="flex-1"
          options={multiSelectOptions}
          onValueChange={onLocationChange}
          defaultValue={selectedLocationIds}
          placeholder="Выберите локации"
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
    </>
  );
}
