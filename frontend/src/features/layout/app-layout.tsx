"use client";

import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import Header from "../header/header";
import AppSideBar from "../sidebar/app.sidebar";
import { queryClient } from "@/shared/api/query-client";

export default function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <QueryClientProvider client={queryClient}>
      <SidebarProvider>
        <div className="flex flex-col h-screen w-full">
          <Header />
          <div className="flex flex-1">
            <AppSideBar />
            <main className="flex-1 p-10">{children}</main>
          </div>
        </div>
      </SidebarProvider>
    </QueryClientProvider>
  );
}
