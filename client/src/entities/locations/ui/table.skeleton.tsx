import { Skeleton } from "@/shared/components/ui/skeleton";

interface LocationTableSkeletonProps {
  rows?: number;
}

export function LocationTableSkeleton({
  rows = 5,
}: LocationTableSkeletonProps) {
  return (
    <div className="rounded-md border">
      {/* Заголовки */}
      <div className="flex h-10 items-center border-b px-4 bg-muted/30">
        <Skeleton className="h-4 w-45" />
        <Skeleton className="h-4 w-30 ml-auto" />
        <Skeleton className="h-4 w-25 ml-4" />
        <Skeleton className="h-4 w-30 ml-4" />
      </div>

      {/* Строки данных */}
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="flex h-12 items-center border-b px-4">
          <Skeleton className="h-4 w-45" />
          <Skeleton className="h-4 w-30 ml-auto" />
          <Skeleton className="h-4 w-25 ml-4" />
          <Skeleton className="h-4 w-30 ml-4" />
        </div>
      ))}
    </div>
  );
}
