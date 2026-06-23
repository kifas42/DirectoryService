import { useQueryClient, QueryKey } from "@tanstack/react-query";
import { AlertCircle, RefreshCw } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "./ui/alert";
import { cn } from "../lib/utils";
import { Button } from "./ui/button";

interface QueryErrorAlertProps {
  message: string;
  queryKey: QueryKey;
  className?: string;
}

export function QueryErrorAlert({
  message,
  queryKey,
  className,
}: QueryErrorAlertProps) {
  const queryClient = useQueryClient();

  const handleRetry = () => {
    queryClient.invalidateQueries({ queryKey });
  };

  return (
    <Alert variant="destructive" className={cn("max-w-lg", className)}>
      <AlertCircle className="h-4 w-4" />
      <AlertTitle>Ошибка загрузки</AlertTitle>
      <AlertDescription className="mt-2 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <span>{message}</span>
        <Button
          variant="outline"
          size="sm"
          className="bg-transparent hover:bg-destructive/20 text-destructive border-destructive/50"
          onClick={handleRetry}
        >
          <RefreshCw className="mr-2 h-3 w-3" />
          Повторить
        </Button>
      </AlertDescription>
    </Alert>
  );
}
