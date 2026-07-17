"use client";

import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import AppSidebar from "../sidebar/app.sidebar";
import Header from "../header/header";
import { getQueryClient } from "@/shared/api/query-client";
import { Toaster } from "@/shared/components/ui/sonner";

export function Layout({ children }: Readonly<{ children: React.ReactNode }>) {
  const queryClient = getQueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      <SidebarProvider>
        <AppSidebar />
        <div className="flex flex-col w-full">
          <Header />
          <main className="p-10 flex-1">{children}</main>
          <Toaster position="top-center" duration={3000} richColors={true} />
        </div>
      </SidebarProvider>
    </QueryClientProvider>
  );
}
