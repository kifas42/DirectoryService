import { UseFormSetError, Path, FieldValues } from "react-hook-form";
import { toast } from "sonner";
import { isEnvelopeError } from "./error";

export function handleApiError<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fieldMap: Record<string, Path<T>>,
) {
  if (process.env.NODE_ENV === "development") {
    console.error("[API Error Diagnostic]", error);
  }

  if (!isEnvelopeError(error)) {
    toast.error("Сетевая ошибка или непредвиденный сбой.");
    return;
  }

  let hasFieldError = false;

  error.messages.forEach((msg) => {
    // Приводим и поле от бэкенда, и результат из карты к типу Path<T>
    const fieldName = (msg.invalidField as Path<T>) || fieldMap[msg.code];

    if (fieldName) {
      setError(fieldName, {
        type: "server",
        message: msg.message,
      });
      hasFieldError = true;
    }
  });

  if (!hasFieldError) {
    toast.error(error.firstMessage || "Произошла ошибка на сервере");
  }
}
