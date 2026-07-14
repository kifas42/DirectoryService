import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { Skeleton } from "@/shared/components/ui/skeleton";

interface SkeletonProps {
  rows?: number;
}

export function LocationTableSkeleton({ rows = 6 }: SkeletonProps) {
  return (
    <div className="rounded-lg border bg-card">
      {/* table-fixed и точно такие же ширины, как в реальной таблице */}
      <Table className="table-fixed w-full">
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead className="w-[25%]">Название</TableHead>
            <TableHead className="w-[35%]">Адрес</TableHead>
            <TableHead className="w-[15%]">Часовой пояс</TableHead>
            <TableHead className="w-[15%]">Дата создания</TableHead>
            <TableHead className="w-[10%] text-right">Статус</TableHead>
            <TableHead className="w-30"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: rows }).map((_, i) => (
            <TableRow key={i} className="hover:bg-transparent">
              <TableCell>
                <Skeleton className="h-5 w-[80%]" />
              </TableCell>

              <TableCell>
                <div className="flex flex-col gap-1.5">
                  <Skeleton className="h-4 w-[90%]" />
                  <Skeleton className="h-3 w-[40%]" />
                  <Skeleton className="h-3 w-[60%]" />
                </div>
              </TableCell>

              <TableCell>
                <div className="flex flex-col gap-1.5">
                  <Skeleton className="h-4 w-[50%]" />
                  <Skeleton className="h-3 w-[70%]" />
                </div>
              </TableCell>

              <TableCell>
                <Skeleton className="h-4 w-20" />
              </TableCell>

              <TableCell className="text-right">
                <div className="flex justify-end">
                  <Skeleton className="h-6 w-17.5 rounded-full" />
                </div>
              </TableCell>

              <TableCell>
                <div className="flex justify-end gap-2">
                  <Skeleton className="h-9 w-9 rounded-md" />
                  <Skeleton className="h-9 w-9 rounded-md" />
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
