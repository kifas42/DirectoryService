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

interface LocationTableProps {
  locations: GetLocationDto[];
}

export function LocationTable({ locations }: LocationTableProps) {
  if (locations.length === 0) {
    return (
      <div className="flex h-32 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
        Локации не найдены
      </div>
    );
  }

  return (
    <div className="rounded-lg border bg-card">
      <Table>
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead>Название</TableHead>
            <TableHead>Адрес</TableHead>
            <TableHead>Часовой пояс</TableHead>
            <TableHead>Дата создания</TableHead>
            <TableHead className="text-right">Статус</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {locations.map((loc) => (
            <TableRow
              key={loc.id}
              className="hover:bg-muted/50 transition-colors"
            >
              <TableCell className="font-medium max-w-55 truncate">
                {loc.name}
              </TableCell>

              <TableCell>
                <div className="flex flex-col gap-0.5">
                  <span className="text-sm leading-none">{loc.street}</span>
                  {(loc.buildingNumber || loc.officeNumber) && (
                    <span className="text-xs text-muted-foreground">
                      {loc.buildingNumber}
                      {loc.buildingNumber && loc.officeNumber && " · "}
                      {loc.officeNumber}
                    </span>
                  )}
                  <span className="text-xs text-muted-foreground">
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
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
