import { SidebarTrigger } from "@/shared/components/ui/sidebar";
import { ModeToggle } from "../theme/mode-toggle";

export default function Header() {
  return (
    <header className="sticky top-0 z-10 flex h-16 w-full items-center justify-between border-b bg-background px-4">
      {/* Левая часть */}
      <div className="flex items-center gap-2">
        <SidebarTrigger />
        <div className="h-6 w-px bg-border mx-2 hidden md:block" />{" "}
        {/* Разделитель */}
        <span className="font-semibold tracking-tight">Directory Service</span>
      </div>

      {/* Правая часть */}
      <div className="flex items-center gap-4">
        <ModeToggle />
        <button className="text-sm font-medium hover:underline">Профиль</button>
        <div className="h-8 w-8 rounded-full bg-muted" />{" "}
        {/* Заглушка под аватар */}
      </div>
    </header>
  );
}
