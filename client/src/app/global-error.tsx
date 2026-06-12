"use client";

import { Button } from "@/shared/components/ui/button";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <html>
      <body>
        <div className="flex min-h-screen flex-col items-center justify-center p-4">
          <h2 className="text-2xl font-bold text-destructive mb-4">
            Критическая ошибка приложения
          </h2>
          <p className="mb-4 text-muted-foreground">{error.message}</p>
          <Button onClick={() => reset()}>Перезагрузить</Button>
        </div>
      </body>
    </html>
  );
}
