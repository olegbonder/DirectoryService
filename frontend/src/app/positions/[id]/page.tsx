import PositionDetail from "@/features/positions/position-detail";

type Props = {
  params: Promise<{ id: string }>;
};

export default async function PositionPage({ params }: Props) {
  const { id } = await params;

  return <PositionDetail id={id} />;
}
