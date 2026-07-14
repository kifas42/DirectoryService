import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { Badge } from "@/shared/components/ui/badge";
import { GetLocationDto } from "@/entities/locations/types";
import { SquarePen, Trash2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";

interface LocationTableProps {
  locations: GetLocationDto[];
  onEdit: (location: GetLocationDto) => void;
  onDelete?: (location: GetLocationDto) => void;
}

export function LocationTable({
  locations,
  onEdit,
  onDelete,
}: LocationTableProps) {
  if (locations.length === 0) {
    return (
      <div className="flex h-32 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
        Локации не найдены
      </div>
    );
  }

  return (
    <div className="rounded-lg border bg-card">
      {/* table-layout: fixed гарантирует, что ширина колонок будет строго по Head */}
      <Table className="table-fixed w-full">
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            {/* Задаем фиксированную или процентную ширину каждой колонке */}
            <TableHead className="w-[25%]">Название</TableHead>
            <TableHead className="w-[35%]">Адрес</TableHead>
            <TableHead className="w-[15%]">Часовой пояс</TableHead>
            <TableHead className="w-[15%]">Дата создания</TableHead>
            <TableHead className="w-[10%] text-right">Статус</TableHead>
            <TableHead className="w-30"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {locations.map((loc) => (
            <TableRow
              key={loc.id}
              className="hover:bg-muted/50 transition-colors"
            >
              <TableCell className="font-medium truncate max-w-0">
                {loc.name}
              </TableCell>

              <TableCell className="truncate max-w-0">
                <div className="flex flex-col gap-0.5 truncate">
                  <span className="text-sm leading-none truncate">
                    {loc.street}
                  </span>
                  {(loc.buildingNumber || loc.officeNumber) && (
                    <span className="text-xs text-muted-foreground">
                      {loc.buildingNumber}
                      {loc.buildingNumber && loc.officeNumber && " · "}
                      {loc.officeNumber}
                    </span>
                  )}
                  <span className="text-xs text-muted-foreground truncate">
                    {[loc.city, loc.stateOrProvince, loc.postalCode]
                      .filter(Boolean)
                      .join(", ")}
                  </span>
                </div>
              </TableCell>

              <TableCell>
                <div className="flex flex-col gap-0.5">
                  <span className="text-sm">{loc.country}</span>
                  <span className="text-xs font-mono text-muted-foreground">
                    {loc.timezone}
                  </span>
                </div>
              </TableCell>

              <TableCell className="text-sm text-muted-foreground whitespace-nowrap">
                {new Date(loc.createdAt).toLocaleDateString("ru-RU", {
                  day: "2-digit",
                  month: "2-digit",
                  year: "numeric",
                })}
              </TableCell>

              <TableCell className="text-right">
                <Badge variant={loc.isActive ? "default" : "secondary"}>
                  {loc.isActive ? "Активна" : "Неактивна"}
                </Badge>
              </TableCell>

              <TableCell className="text-right whitespace-nowrap">
                <Button
                  variant="secondary"
                  size="icon"
                  className="mr-2"
                  onClick={() => onEdit(loc)}
                >
                  <SquarePen className="h-4 w-4" />
                </Button>
                <Button
                  variant="destructive"
                  size="icon"
                  onClick={() => onDelete?.(loc)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
