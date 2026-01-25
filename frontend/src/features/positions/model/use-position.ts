import { positionsQueryOptions } from "@/entities/positions/api";
import { useQuery } from "@tanstack/react-query";

export default function usePosition(positionId: string) {
  const { data, isPending, error, isError, refetch } = useQuery({
    ...positionsQueryOptions.getPositionOptions(positionId),
  });

  return {
    position: data,
    isPending,
    error,
    isError,
    refetch,
  };
}
