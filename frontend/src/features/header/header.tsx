import {
  Avatar,
  AvatarFallback,
  AvatarImage,
} from "../../shared/components/ui/avatar";
import Link from "next/link";
import { routes } from "@/shared/routes";
import { SidebarTrigger } from "../../shared/components/ui/sidebar";
import { Input } from "@/shared/components/ui/input";
import {
  setGlobalFilterSearch,
  useGetGlobalFilter,
} from "@/shared/stores/global-search-store";

export default function Header() {
  const globalSearch = useGetGlobalFilter();
  return (
    <header className="sticky top-0 z-50 w-full bg-white border-b border-gray-200 shadow-sm">
      <div className="flex items-center justify-between px-6 py-4">
        {/* Левая часть - Логотип и toggle */}
        <div className="flex items-center gap-4">
          <SidebarTrigger className="lg:hidden p-2 hover:bg-gray-100 rounded-lg transition-colors" />
          <Link
            href={routes.home}
            className="flex items-center gap-3 hover:opacity-80 transition-opacity"
          >
            <div className="w-10 h-10 bg-blue-600 rounded-lg flex items-center justify-center text-white font-bold text-lg shadow-sm">
              DS
            </div>
            <div className="hidden sm:block">
              <h1 className="text-lg font-semibold text-gray-900">
                Directory Service
              </h1>
            </div>
          </Link>
        </div>

        {/* Средняя часть - Поиск */}
        <div className="flex items-center gap-4">
          <Input
            type="search"
            placeholder="Поиск"
            className="w-80"
            value={globalSearch}
            onChange={(e) => setGlobalFilterSearch(e.target.value)}
          />
        </div>

        {/* Правая часть - Профиль */}
        <div className="flex items-center gap-4">
          <Avatar className="w-9 h-9 cursor-pointer hover:opacity-80 transition-colors border border-gray-200">
            <AvatarImage src="https://github.com/shadcn.png" alt="User" />
            <AvatarFallback className="bg-blue-600 text-white">
              ИП
            </AvatarFallback>
          </Avatar>
          <div className="hidden md:flex flex-col text-right">
            <p className="text-sm font-medium text-gray-900">Иван Петров</p>
            <p className="text-xs text-gray-500">Администратор</p>
          </div>
        </div>
      </div>
    </header>
  );
}
