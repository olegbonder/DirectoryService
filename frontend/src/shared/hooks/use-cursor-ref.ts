import { RefCallback, useCallback } from "react";

type UseCursorRefOptions = {
  hasNextPage: boolean;
  isFetchingNextPage: boolean;
  fetchNextPage: () => void;
};
export default function useCursorRef({
  hasNextPage,
  isFetchingNextPage,
  fetchNextPage,
}: UseCursorRefOptions) {
  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (node) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        {
          threshold: 0.5,
        },
      );

      if (node) {
        observer.observe(node);
      }

      return () => observer.disconnect();
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage],
  );

  return cursorRef;
}
