"use client";

import PositionDetail from "@/features/positions/position-detail";
import React from "react";

export default function PositionPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = React.use(params);

  return <PositionDetail id={id} />;
}
