import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";
import { Button } from "@/shared/components/ui/button";
import { GetLocationDto } from "@/entities/locations/types";
import { Loader2 } from "lucide-react";

interface DeleteLocationDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  location: GetLocationDto;
  onConfirm: () => void;
  isPending: boolean;
  error?: string;
}

export default function DeleteLocationDialog({
  open,
  onOpenChange,
  location,
  onConfirm,
  isPending,
  error,
}: DeleteLocationDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Удалить локацию?</AlertDialogTitle>
          <div className="text-sm text-muted-foreground space-y-4 pt-2">
            <div>
              Вы собираетесь удалить локацию{" "}
              <span className="font-semibold text-foreground">
                «{location.name}»
              </span>
              .
            </div>
          </div>
        </AlertDialogHeader>

        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>Отмена</AlertDialogCancel>
          <Button
            variant="destructive"
            onClick={onConfirm}
            disabled={isPending}
          >
            {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Удалить
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
