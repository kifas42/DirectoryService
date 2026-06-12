"use client";

import { useEffect } from "react";
import { Button } from "@/shared/components/ui/button";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/shared/components/ui/alert";
import { AlertCircle } from "lucide-react";

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("Frontend Error:", error);
  }, [error]);

  return (
    <div className="flex min-h-[50vh] items-center justify-center p-4">
      <Alert variant="destructive" className="max-w-md w-full">
        <AlertCircle className="h-4 w-4" />
        <AlertTitle>Произошла ошибка</AlertTitle>
        <AlertDescription className="mt-2">
          <p className="mb-4 text-sm text-muted-foreground">
            {error.message || "Непредвиденная ошибка при загрузке компонента."}
            {error.digest && (
              <span className="block mt-2 text-xs opacity-70">
                ID: {error.digest}
              </span>
            )}
          </p>
          <div className="flex gap-2">
            <Button onClick={() => reset()} variant="outline">
              Попробовать снова
            </Button>
            <Button onClick={() => window.location.reload()}>
              Перезагрузить страницу
            </Button>
          </div>
        </AlertDescription>
      </Alert>
    </div>
  );
}
