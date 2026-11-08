"use client";

import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import AppSidebar from "../sidebar/app.sidebar";
import Header from "../header/header";
import { getQueryClient } from "@/shared/api/query-client";
import { Toaster } from "@/shared/components/ui/sonner";
import { ThemeProvider } from "@/shared/components/theme-provider";

export function Layout({ children }: Readonly<{ children: React.ReactNode }>) {
  const queryClient = getQueryClient();

  return (
    <ThemeProvider
      attribute="class"
      defaultTheme="system"
      enableSystem
      disableTransitionOnChange
    >
      <QueryClientProvider client={queryClient}>
        <SidebarProvider>
          <AppSidebar />
          <div className="flex min-h-screen w-full flex-col">
            <Header />
            <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8">
              <div className="page-shell">{children}</div>
            </main>
            <Toaster position="top-center" duration={3000} richColors={true} />
          </div>
        </SidebarProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}
