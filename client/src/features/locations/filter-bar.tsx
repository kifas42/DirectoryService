import React from "react";
import { Input } from "@/shared/components/ui/input";
import { Button } from "@/shared/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Search, ArrowDownAZ, ArrowUpZA } from "lucide-react";

interface FilterBarProps {
  search?: string;
  onSearchChange: (value: string) => void;
  sortBy?: string;
  onSortByChange: (value: string) => void;
  sortOrder: "asc" | "desc";
  onSortOrderChange: (value: "asc" | "desc") => void;
  children?: React.ReactNode;
}

export function FilterBar({
  search,
  onSearchChange,
  sortBy,
  onSortByChange,
  sortOrder,
  onSortOrderChange,
  children,
}: FilterBarProps) {
  return (
    <div className="flex flex-col sm:flex-row items-center gap-3 w-full max-w-4xl">
      {children && <div className="w-full md:w-auto shrink-0">{children}</div>}
      <div className="relative w-full sm:flex-1">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Поиск ..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="pl-9"
        />
      </div>

      <div className="flex items-center gap-2 w-full sm:w-auto shrink-0">
        <Select
          value={sortBy}
          onValueChange={(val) => {
            if (val !== null) onSortByChange(val);
          }}
        >
          <SelectTrigger className="w-full sm:w-40">
            <SelectValue placeholder="Сортировка" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="date">По дате</SelectItem>
            <SelectItem value="name">По имени</SelectItem>
          </SelectContent>
        </Select>

        <Button
          variant="outline"
          size="icon"
          onClick={() =>
            onSortOrderChange(sortOrder === "asc" ? "desc" : "asc")
          }
          aria-label="Изменить направление сортировки"
          className="shrink-0"
        >
          {sortOrder === "asc" ? (
            <ArrowDownAZ className="h-4 w-4" />
          ) : (
            <ArrowUpZA className="h-4 w-4" />
          )}
        </Button>
      </div>
    </div>
  );
}
