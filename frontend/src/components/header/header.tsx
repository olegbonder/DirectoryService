import { Avatar, AvatarFallback, AvatarImage } from "../ui/avatar";
import Link from "next/link";
import { routes } from "@/shared/routes";
import { SidebarTrigger } from "../ui/sidebar";

export default function Header() {
  return (
    <header className="sticky top-0 z-50 w-full bg-background/95">
      <div className="flex items-center justify-between px-4 py-4">
        {/* Левая часть - Логотип*/}
        <div className="flex items-center gap-2">
          <SidebarTrigger className="md:hidden mb-4" />
          <div className="flex items-center gap-2 font-semibold text-xl">
            <div className="h-8 w-8 bg-red-600 rounded-full flex items-center">
              FS
            </div>
            <Link href={routes.home}>
              <span>FullStack</span>
            </Link>
          </div>
        </div>

        {/* Правая часть - Аватарка */}
        <div className="flex items-center gap-4">
          <Avatar className="h-9 w-9 cursor-pointer hover:opacity-80 transition-colors">
            <AvatarImage src="https://github.com/shadcn.png" alt="User" />
            <AvatarFallback>ST</AvatarFallback>
          </Avatar>
        </div>
      </div>
    </header>
  );
}
