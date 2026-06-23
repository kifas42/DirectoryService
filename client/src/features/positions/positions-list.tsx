import { GetPositionDto } from "@/entities/positions/types";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Briefcase, Calendar, CheckCircle2, XCircle } from "lucide-react";

interface PositionListProps {
  positions: GetPositionDto[];
}

export default function PositionsList({ positions }: PositionListProps) {
  if (!positions || positions.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center p-8 text-center border-2 border-dashed rounded-xl border-muted-foreground/20 bg-muted/30">
        <Briefcase className="w-10 h-10 mb-4 text-muted-foreground/60" />
        <h3 className="text-lg font-semibold">Список позиций пуст</h3>
        <p className="text-sm text-muted-foreground">
          Добавьте новые позиции, чтобы они появились здесь.
        </p>
      </div>
    );
  }

  return (
    // Вертикальный стек (flex-col) с отступами между карточками
    <div className="flex flex-col gap-4 w-full max-w-2xl mx-auto">
      {positions.map((position) => (
        <Card
          key={position.id}
          className="transition-all hover:shadow-md border-muted/60"
        >
          <CardHeader className="pb-3">
            <div className="flex items-start justify-between gap-4">
              <div className="space-y-1">
                <CardTitle className="text-xl font-bold tracking-tight text-foreground">
                  {position.name}
                </CardTitle>
                <CardDescription className="text-sm text-muted-foreground leading-relaxed pt-1">
                  {position.description}
                </CardDescription>
              </div>

              {/* Статус-бейдж */}
              <div
                className={`flex items-center gap-1.5 px-2.5 py-1 text-xs font-medium rounded-full shrink-0 ${
                  position.isActive
                    ? "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400"
                    : "bg-destructive/10 text-destructive"
                }`}
              >
                {position.isActive ? (
                  <>
                    <CheckCircle2 className="w-3.5 h-3.5" />
                    <span>Активна</span>
                  </>
                ) : (
                  <>
                    <XCircle className="w-3.5 h-3.5" />
                    <span>Неактивна</span>
                  </>
                )}
              </div>
            </div>
          </CardHeader>

          <CardContent className="pt-0 flex items-center justify-end text-xs text-muted-foreground border-t border-muted/40 mt-2 py-3 bg-muted/10 rounded-b-xl">
            <div className="flex items-center gap-1.5">
              <Calendar className="w-3.5 h-3.5" />
              <span>
                Создано:{" "}
                {new Date(position.createdAt).toLocaleDateString("ru-RU", {
                  day: "numeric",
                  month: "long",
                  year: "numeric",
                })}
              </span>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
