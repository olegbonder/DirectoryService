import * as React from "react";
import { CheckIcon, XIcon, ChevronDownIcon, ChevronUpIcon } from "lucide-react";
import { RefCallback, useCallback } from "react";

import { cn } from "@/shared/lib/utils";
import { Button } from "@/shared/components/ui/button";
import { Badge } from "@/shared/components/ui/badge";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/shared/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/shared/components/ui/command";
import { Spinner } from "./spinner";

interface MultiSelectOption {
  label: string;
  value: string;
  disabled?: boolean;
}

interface MultiSelectProps {
  options: MultiSelectOption[];
  onValueChange: (value: string[]) => void;
  defaultValue?: string[];
  placeholder?: string;
  className?: string;
  disabled?: boolean;
  loadMore?: () => void;
  hasNextPage?: boolean;
  isLoadingMore?: boolean;
  maxDisplayCount?: number; // Количество отображаемых бэйджей до появления "ещё N"
  searchPlaceholder?: string; // Плейсхолдер для поля поиска
  notFoundPlaceholder?: string; // Плейсхолдер, если ничего не нашлось
  morePlaceholder?: string;
}

const MultiSelect = React.forwardRef<
  React.ElementRef<typeof PopoverTrigger>,
  MultiSelectProps
>(
  (
    {
      options,
      onValueChange,
      defaultValue = [],
      placeholder = "Select options",
      className,
      disabled = false,
      loadMore,
      hasNextPage,
      isLoadingMore,
      maxDisplayCount = 3, // По умолчанию показываем 3 бэйджа, как в оригинальном компоненте
      searchPlaceholder = "Search...",
      notFoundPlaceholder = "No results found",
      morePlaceholder = "more",
    },
    ref,
  ) => {
    const [selectedValues, setSelectedValues] =
      React.useState<string[]>(defaultValue);
    const [isOpen, setIsOpen] = React.useState(false);

    const handleSelect = (value: string) => {
      if (disabled) return;
      const newSelectedValues = selectedValues.includes(value)
        ? selectedValues.filter((v) => v !== value)
        : [...selectedValues, value];

      setSelectedValues(newSelectedValues);
      onValueChange(newSelectedValues);
    };

    const handleRemove = (value: string) => {
      if (disabled) return;
      const newSelectedValues = selectedValues.filter((v) => v !== value);
      setSelectedValues(newSelectedValues);
      onValueChange(newSelectedValues);
    };

    const handleClear = () => {
      if (disabled) return;
      setSelectedValues([]);
      onValueChange([]);
    };

    // Infinity scroll logic
    const sentinelRef: RefCallback<HTMLDivElement> = useCallback(
      (node) => {
        if (!hasNextPage || isLoadingMore || !loadMore) return;

        const observer = new IntersectionObserver(
          (entries) => {
            if (entries[0].isIntersecting) {
              loadMore();
            }
          },
          { threshold: 1.0 },
        );

        if (node) {
          observer.observe(node);
        }

        return () => observer.disconnect();
      },
      [loadMore, hasNextPage, isLoadingMore],
    );

    return (
      <Popover open={isOpen} onOpenChange={setIsOpen}>
        <PopoverTrigger asChild>
          <Button
            ref={ref}
            variant="outline"
            className={cn(
              "flex h-9 w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50",
              "min-h-9 w-full flex-wrap justify-start gap-1 hover:bg-background",
              className,
            )}
            disabled={disabled}
          >
            <div className="flex flex-wrap gap-1 grow min-w-0">
              {selectedValues.length > 0 ? (
                <>
                  {selectedValues.slice(0, maxDisplayCount).map((value) => {
                    const option = options.find((opt) => opt.value === value);
                    return (
                      <Badge
                        key={value}
                        variant="secondary"
                        className="mr-1 hover:bg-secondary/60 transition-colors hover:-translate-y-1 hover:scale-110"
                      >
                        {option?.label}
                        <div
                          className="inline-block ml-1 cursor-pointer hover:text-red-500 transition-colors"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleRemove(value);
                          }}
                          onMouseDown={(e) => {
                            e.stopPropagation();
                          }}
                        >
                          <XIcon className="h-3 w-3" />
                        </div>
                      </Badge>
                    );
                  })}
                  {selectedValues.length > maxDisplayCount && (
                    <Badge variant="secondary" className="mr-1">
                      + {selectedValues.length - maxDisplayCount}{" "}
                      {morePlaceholder}
                    </Badge>
                  )}
                </>
              ) : (
                <span className="text-muted-foreground ml-1 truncate text-sm">
                  {placeholder}
                </span>
              )}
            </div>
            <div className="flex items-center gap-1 shrink-0">
              {selectedValues.length > 0 && (
                <div
                  className="h-4 w-4 cursor-pointer text-muted-foreground hover:text-foreground flex items-center justify-center"
                  onClick={(e) => {
                    e.stopPropagation();
                    handleClear();
                  }}
                  onMouseDown={(e) => {
                    e.preventDefault(); // Prevent blur/focus events that might interfere
                  }}
                  aria-label="Clear all selections"
                  role="button"
                  tabIndex={0}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      handleClear();
                    }
                  }}
                >
                  <XIcon className="h-4 w-4" />
                </div>
              )}
              {isOpen ? (
                <ChevronUpIcon className="h-4 w-4 text-muted-foreground" />
              ) : (
                <ChevronDownIcon className="h-4 w-4 text-muted-foreground" />
              )}
            </div>
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-full p-0" align="start">
          <Command>
            <CommandInput placeholder={searchPlaceholder} />
            <CommandList>
              <CommandEmpty>{notFoundPlaceholder}</CommandEmpty>
              <CommandGroup>
                {options.map((option) => {
                  const isSelected = selectedValues.includes(option.value);
                  return (
                    <CommandItem
                      key={option.value}
                      onSelect={() => handleSelect(option.value)}
                      disabled={option.disabled}
                      className="cursor-pointer"
                    >
                      <div
                        className={cn(
                          "mr-2 flex h-4 w-4 items-center justify-center rounded-sm border border-primary",
                          isSelected
                            ? "bg-primary text-primary-foreground"
                            : "opacity-50 [&_svg]:invisible",
                        )}
                      >
                        <CheckIcon className="h-4 w-4" />
                      </div>
                      {option.label}
                    </CommandItem>
                  );
                })}
                {loadMore && hasNextPage && (
                  <div ref={sentinelRef} className="py-2">
                    {isLoadingMore ? (
                      <div className="flex items-center justify-center">
                        <Spinner />
                      </div>
                    ) : (
                      <div className="h-4" />
                    )}
                  </div>
                )}
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>
    );
  },
);

MultiSelect.displayName = "MultiSelect";

export { MultiSelect };
export type { MultiSelectOption };
