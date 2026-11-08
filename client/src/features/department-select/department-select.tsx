"use client";

import { GetDepartmentDto } from "@/entities/departments/types";
import { Input } from "@/shared/components/ui/input";
import { Button } from "@/shared/components/ui/button";
import { Badge } from "@/shared/components/ui/badge";
import { ActivityFilter } from "@/shared/components/ui/activity-filter";
import React, { useState } from "react";
import { useDebouncedCallback } from "use-debounce";
import { useDepartmentsLists } from "./model/use-departments-list";
import { QueryErrorAlert } from "@/shared/components/query-error-alert";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Spinner } from "@/shared/components/ui/spinner";

interface DepartmentSelectProps {
  selectedDepartments: GetDepartmentDto[];
  onChange: (value: GetDepartmentDto[]) => void;
  mode: "single" | "multiple";
  excludeIds: string[];
}

export default function DepartmentSelect({
  selectedDepartments,
  onChange,
  mode,
  excludeIds,
}: DepartmentSelectProps) {
  const [search, setSearch] = useState("");
  const applySearch = useDebouncedCallback((value: string) => {
    setAppliedSearch(value);
    setPage(1);
  }, 500);
  const [appliedSearch, setAppliedSearch] = useState("");
  const [isActive, setIsActive] = useState<boolean | undefined>(true);
  const [page, setPage] = useState(1);

  const {
    departments,
    totalPages,
    isPending,
    isFetching,
    isPlaceholderData,
    isError,
    error,
  } = useDepartmentsLists({
    search: appliedSearch,
    isActive,
    page,
    pageSize: 2,
    excludeIds,
  });
  const isSearchPending = search !== appliedSearch;
  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <p className="text-sm font-medium">Выбранные подразделения</p>
        {selectedDepartments.length > 0 ? (
          <div
            className="flex flex-wrap gap-2"
            aria-label="Выбранные подразделения"
          >
            {selectedDepartments.map((department) => (
              <Badge
                key={department.id}
                onClick={() =>
                  onChange(
                    selectedDepartments.filter(
                      (selected) => selected.id !== department.id,
                    ),
                  )
                }
                variant="secondary"
              >
                {department.name}
              </Badge>
            ))}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">Ничего не выбрано</p>
        )}
      </div>

      {isPending && !isError && (
        <div
          role="status"
          aria-label="Загрузка подразделений"
          className="space-y-3"
        >
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Spinner aria-hidden="true" />
            <span>Загрузка подразделений...</span>
          </div>
          <div className="flex flex-wrap gap-2" aria-hidden="true">
            <Skeleton className="h-5 w-28" />
            <Skeleton className="h-5 w-36" />
            <Skeleton className="h-5 w-24" />
          </div>
        </div>
      )}

      {isFetching && !isPending && (
        <div
          role="status"
          className="flex items-center gap-2 text-sm text-muted-foreground"
        >
          <Spinner aria-hidden="true" />
          <span>Обновление подразделений...</span>
        </div>
      )}

      {isError && (
        <QueryErrorAlert
          message={error ? error.message : "Не удалось загрузить данные"}
          queryKey={[
            "departments",
            { search: appliedSearch, isActive, page, pageSize: 10, excludeIds },
          ]}
        />
      )}
      <Input
        placeholder="Поиск подразделений..."
        aria-label="Поиск подразделений"
        value={search}
        onChange={(e) => {
          const value = e.target.value;
          setSearch(value);
          applySearch(value);
        }}
      />
      <ActivityFilter
        value={isActive}
        onChange={(value) => {
          setIsActive(value);
          setPage(1);
        }}
      />

      {departments && !isError && (
        <div className="space-y-2">
          <p className="text-sm font-medium">Найденные подразделения</p>
          {departments.length > 0 ? (
            <ul
              className={`divide-y rounded-xl border transition-opacity ${isPlaceholderData ? "opacity-50" : "opacity-100"}`}
              aria-busy={isFetching}
              aria-label="Найденные подразделения"
            >
              {departments.map((department) => {
                const isSelected = selectedDepartments.some(
                  (selected) => selected.id === department.id,
                );

                return (
                  <li
                    key={department.id}
                    className="flex items-center justify-between gap-3 px-3 py-2"
                  >
                    <span className="min-w-0 wrap-break-word text-sm">
                      {department.name}
                    </span>
                    <div className="flex shrink-0 items-center gap-3">
                      {selectedDepartments.some(
                        (selected) => selected.id === department.id,
                      ) && (
                        <span className="text-xs text-muted-foreground">
                          Выбрано
                        </span>
                      )}
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        disabled={isSelected}
                        onClick={() => {
                          if (!isSelected) {
                            if (mode === "single") {
                              onChange([department]);
                            } else {
                              onChange([...selectedDepartments, department]);
                            }
                          }
                        }}
                      >
                        Выбрать
                      </Button>
                    </div>
                  </li>
                );
              })}
            </ul>
          ) : (
            <p role="status" className="text-sm text-muted-foreground">
              Подразделения не найдены
            </p>
          )}
        </div>
      )}

      <nav
        aria-label="Страницы подразделений"
        className="flex items-center gap-3"
      >
        <Button
          type="button"
          variant="outline"
          disabled={isSearchPending || page === 1 || isFetching || isPending}
          onClick={() => setPage((current) => Math.max(1, current - 1))}
        >
          Назад
        </Button>
        <span className="text-sm" aria-live="polite">
          Страница {page}
        </span>
        <Button
          type="button"
          variant="outline"
          disabled={
            isSearchPending ||
            isFetching ||
            isPending ||
            isError ||
            totalPages === undefined ||
            page >= totalPages
          }
          onClick={() => setPage((current) => current + 1)}
        >
          Далее
        </Button>
      </nav>
    </div>
  );
}
