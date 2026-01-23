"use client";

import { routes } from "@/shared/routes";
import {
  Home,
  ListTodo,
  MapPin,
  PersonStanding,
  Plus,
  Share2,
} from "lucide-react";
import Link from "next/link";
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
import { usePathname } from "next/navigation";

const menuItems = [
  { href: routes.home, label: "Главная", icon: Home },
  { href: routes.locations, label: "Локации", icon: MapPin },
  { href: routes.departments, label: "Отделы", icon: Share2 },
  { href: routes.positions, label: "Должности", icon: PersonStanding },
];

export default function AppSideBar() {
  const pathname = usePathname();

  const { isMobile, setOpenMobile } = useSidebar();

  return (
    <Sidebar collapsible="icon" className="mt-10">
      <SidebarHeader className="pl-5"></SidebarHeader>
      <SidebarContent className="px-3 py-4">
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu className="space-y-1">
              {menuItems.map((item) => {
                const isActive =
                  pathname === item.href ||
                  pathname.startsWith(item.href + "/");

                return (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      asChild
                      isActive={isActive}
                      tooltip={item.label}
                      className="hover:bg-gray-300 transition-colors"
                      onClick={() => setOpenMobile(false)}
                    >
                      <Link
                        href={item.href}
                        className="flex items-center gap-3"
                      >
                        <item.icon className="h-5 w-5" />
                        <span>{item.label}</span>
                      </Link>
                    </SidebarMenuButton>
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
