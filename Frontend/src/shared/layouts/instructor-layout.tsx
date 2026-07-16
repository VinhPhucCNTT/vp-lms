import * as React from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import { LayoutDashboardIcon, BookOpenIcon, FileTextIcon, CalendarIcon, BellIcon, LogOutIcon, ChevronDownIcon, CodeIcon } from "lucide-react";
import { useAuth } from "@/features/auth/auth-context";
import { ModeToggle } from "@/components/theme-toggle";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { Sidebar, SidebarContent, SidebarFooter, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem, SidebarProvider, SidebarTrigger, SidebarInset } from "@/components/ui/sidebar";
import { Separator } from "@/components/ui/separator";

const navigation = [
  { title: "Dashboard", url: "/instructor", icon: LayoutDashboardIcon, exact: true },
  { title: "My Courses", url: "/instructor/courses", icon: BookOpenIcon, exact: true },
  { title: "Calendar", url: "/instructor/calendar", icon: CalendarIcon, exact: false },
  { title: "Submissions", url: "/instructor/submissions", icon: FileTextIcon, exact: false },
  { title: "Coding Problems", url: "/instructor/coding-problems", icon: CodeIcon, exact: false },
  { title: "Notifications", url: "/instructor/notifications", icon: BellIcon, exact: false },
];

export function InstructorLayout() {
  const { user, logout } = useAuth();
  const location = useLocation();

  function isActive(item: typeof navigation[number]) {
    if (item.exact) return location.pathname === item.url;
    return location.pathname.startsWith(item.url);
  }

  return (
    <SidebarProvider>
      <Sidebar>
        <SidebarHeader>
          <div className="flex items-center gap-2 px-2 py-1">
            <div className="size-8 rounded-lg bg-primary flex items-center justify-center">
              <BookOpenIcon className="size-4 text-primary-foreground" />
            </div>
            <div className="flex flex-col">
              <span className="text-sm font-bold">LMS Academic</span>
              <span className="text-xs text-muted-foreground">Instructor Portal</span>
            </div>
          </div>
        </SidebarHeader>
        <Separator />
        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupLabel>Navigation</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {navigation.map((item) => (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton asChild isActive={isActive(item)} tooltip={item.title}>
                      <Link to={item.url}>
                        <item.icon className="size-4" />
                        <span>{item.title}</span>
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        </SidebarContent>
        <SidebarFooter>
          <SidebarMenu>
            <SidebarMenuItem>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <SidebarMenuButton className="h-auto p-2">
                    <Avatar size="sm"><AvatarFallback>{user?.firstName?.[0]}{user?.lastName?.[0]}</AvatarFallback></Avatar>
                    <div className="flex flex-col items-start text-left">
                      <span className="text-sm font-medium">{user?.firstName} {user?.lastName}</span>
                      <span className="text-xs text-muted-foreground">Instructor</span>
                    </div>
                    <ChevronDownIcon className="ml-auto size-4" />
                  </SidebarMenuButton>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="start" className="w-[--radix-dropdown-menu-trigger-width]">
                  <DropdownMenuLabel>My Account</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={logout} className="text-destructive"><LogOutIcon className="size-4 mr-2" />Log out</DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarFooter>
      </Sidebar>
      <SidebarInset>
        <header className="flex h-14 items-center justify-between border-b px-4 lg:px-6">
          <div className="flex items-center gap-2"><SidebarTrigger /></div>
          <div className="flex items-center gap-2"><ModeToggle /></div>
        </header>
        <main className="flex-1 p-4 lg:p-6"><Outlet /></main>
      </SidebarInset>
    </SidebarProvider>
  );
}
