"use client";

import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import AppSidebar from "../sidebar/app.sidebar";
import Header from "../header/header";
import { queryClient } from "@/shared/api/query-client";

export function Layout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <QueryClientProvider client={queryClient}>
      <SidebarProvider>
        <AppSidebar />
        <div className="flex flex-col w-full">
          <Header />
          <main className="p-10 flex-1">{children}</main>
        </div>
      </SidebarProvider>
    </QueryClientProvider>
  );
}
