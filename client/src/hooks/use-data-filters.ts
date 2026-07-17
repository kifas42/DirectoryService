import { DataTableFilterParams } from "@/features/locations/model/types";
import { useState } from "react";
import { useDebounce } from "use-debounce";

interface UseDataTableFiltersOptions {
  initialPageSize?: number;
  initialSortBy?: string;
  initialSortOrder?: "asc" | "desc";
}

export function useDataFilters(options: UseDataTableFiltersOptions = {}) {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(options.initialPageSize ?? 10);
  const [search, setSearch] = useState("");
  const [sortBy, setSortBy] = useState<string>(options.initialSortBy ?? "date");
  const [sortOrder, setSortOrder] = useState<"asc" | "desc">(
    options.initialSortOrder ?? "desc",
  );
  const [isActive, setIsActive] = useState<boolean | undefined>(true);

  // Дебаунсим поиск для отправки на бэкенд
  const [debouncedSearch] = useDebounce(search, 500);

  // Сброс страницы на 1 при изменении любых фильтров
  const handleSearchChange = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  const handleSortByChange = (value: string) => {
    setSortBy(value);
    setPage(1);
  };

  const handleSortOrderChange = (value: "asc" | "desc") => {
    setSortOrder(value);
    setPage(1);
  };

  const handleIsActiveChange = (value: boolean | undefined) => {
    setIsActive(value);
    setPage(1);
  };

  // Методы навигации, которые знают про верхнюю границу страниц
  const handleNextPage = (maxPages: number) => {
    if (page < maxPages) setPage((prev) => prev + 1);
  };
  const handlePrevPage = () => {
    if (page > 1) setPage((prev) => prev - 1);
  };

  const apiParams: DataTableFilterParams = {
    page,
    pageSize,
    search: debouncedSearch,
    sortBy,
    sortOrder,
    isActive,
  };

  return {
    // 1. Для FilterBar (передаем чистый search, чтобы инпут реагировал мгновенно)
    search,
    sortBy,
    sortOrder,
    handleSearchChange,
    handleSortByChange,
    handleSortOrderChange,
    isActive,
    handleIsActiveChange,

    // 2. Для DataTablePagination (чистые готовые методы без параметров)
    currentPage: page,
    setPage,
    handleNextPage,
    handlePrevPage,

    // 3. Параметры для передачи в ЛЮБОЙ хук данных / useQuery
    apiParams,
  };
}
