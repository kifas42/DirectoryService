"use client";

import { Tabs, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";

interface ActivityFilterProps {
  value: boolean | undefined;
  onChange: (value: boolean | undefined) => void;
  className?: string;
  inactiveLabel?: string;
}

export function ActivityFilter({
  value,
  onChange,
  className,
  inactiveLabel = "Неактивные",
}: ActivityFilterProps) {
  return (
    <Tabs
      value={value === undefined ? "all" : String(value)}
      onValueChange={(nextValue) => {
        onChange(nextValue === "all" ? undefined : nextValue === "true");
      }}
      className={className}
    >
      <TabsList className="grid w-full grid-cols-3" aria-label="Фильтр активности">
        <TabsTrigger value="all">Все</TabsTrigger>
        <TabsTrigger value="true">Активные</TabsTrigger>
        <TabsTrigger value="false">{inactiveLabel}</TabsTrigger>
      </TabsList>
    </Tabs>
  );
}
