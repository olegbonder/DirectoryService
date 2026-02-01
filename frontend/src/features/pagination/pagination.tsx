import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/shared/components/ui/pagination";

type ItemsPaginationProps = {
  totalPages: number;
  page: number;
  setPage: (page: number) => void;
};
export default function ItemsPagination({
  totalPages,
  page,
  setPage,
}: ItemsPaginationProps) {
  return (
    <div className="flex justify-center">
      <Pagination>
        <PaginationContent>
          <PaginationPrevious
            onClick={() => setPage(Math.max(1, page - 1))}
            text="Предыдущая"
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
            ),
          )}
          <PaginationNext
            onClick={() => setPage(Math.max(totalPages, page + 1))}
            text="Следующая"
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
