import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/shared/components/ui/pagination";
import { Dispatch, SetStateAction } from "react";

export default function LocationsPagination({
  totalPages,
  page,
  setPage,
}: {
  totalPages: number;
  page: number;
  setPage: Dispatch<SetStateAction<number>>;
}) {
  return (
    <div className="mt-8 flex justify-center">
      <Pagination>
        <PaginationContent>
          <PaginationPrevious
            onClick={() => setPage((prev) => Math.max(1, prev - 1))}
            className={
              page === 1 ? "pointer-events-none opacity-50" : "cursor-pointer"
            }
          ></PaginationPrevious>
          {Array.from({ length: totalPages }, (_, i) => i + 1).map(
            (pageNumber) => (
              <PaginationItem key={pageNumber}>
                <PaginationLink
                  className="cursor-pointer"
                  onClick={() => setPage(pageNumber)}
                  isActive={pageNumber === page}
                >
                  {pageNumber}
                </PaginationLink>
              </PaginationItem>
            )
          )}
          <PaginationNext
            onClick={() => setPage((next) => Math.max(totalPages, next + 1))}
            className={
              page === totalPages
                ? "pointer-events-none opacity-50"
                : "cursor-pointer"
            }
          ></PaginationNext>
        </PaginationContent>
      </Pagination>
    </div>
  );
}
