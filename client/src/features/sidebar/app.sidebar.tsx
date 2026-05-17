"use client";
import { BriefcaseBusiness, Contact, Home, MapPin } from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "../../shared/components/ui/sidebar";
import { routes } from "@/shared/routes";
import { usePathname } from "next/navigation";
import Link from "next/link";

const menuItems = [
  { href: routes.home, label: "Главная", icon: Home },
  { href: routes.departments, label: "Подразделения", icon: BriefcaseBusiness },
  { href: routes.locations, label: "Локации", icon: MapPin },
  { href: routes.positions, label: "Позиции", icon: Contact },
];

export default function AppSidebar() {
  const pathname = usePathname();

  const { isMobile, setOpenMobile } = useSidebar();

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader></SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {menuItems.map((item) => {
                const isActive =
                  pathname === item.href ||
                  pathname.startsWith(item.href + "/");

                return (
                  <SidebarMenuItem key={item.href}>
                    <Link href={item.href}>
                      <SidebarMenuButton
                        isActive={isActive}
                        tooltip={item.label}
                        onClick={() => {
                          if (isMobile) setOpenMobile(false);
                        }}
                      >
                        {item.icon && <item.icon />}
                        <span>{item.label}</span>
                      </SidebarMenuButton>
                    </Link>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}
