import * as React from "react";
import { ClockIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface CountdownTimerProps {
  startTime: string;
  durationMinutes: number;
  onExpire?: () => void;
  className?: string;
}

export function CountdownTimer({ startTime, durationMinutes, onExpire, className }: CountdownTimerProps) {
  const [remaining, setRemaining] = React.useState(() => calculateRemaining(startTime, durationMinutes));

  React.useEffect(() => {
    const interval = setInterval(() => {
      const r = calculateRemaining(startTime, durationMinutes);
      setRemaining(r);
      if (r <= 0) {
        clearInterval(interval);
        onExpire?.();
      }
    }, 1000);
    return () => clearInterval(interval);
  }, [startTime, durationMinutes, onExpire]);

  const minutes = Math.floor(remaining / 60);
  const seconds = remaining % 60;
  const isLow = remaining <= 300;
  const isCritical = remaining <= 60;

  return (
    <div
      className={cn(
        "flex items-center gap-2 rounded-lg border px-3 py-2 font-mono text-sm font-medium tabular-nums",
        isCritical
          ? "border-destructive bg-destructive/5 text-destructive"
          : isLow
            ? "border-warning bg-warning/5 text-warning-foreground"
            : "border-border bg-muted/30 text-foreground",
        className
      )}
    >
      <ClockIcon className="size-4" />
      <span>
        {String(minutes).padStart(2, "0")}:{String(seconds).padStart(2, "0")}
      </span>
    </div>
  );
}

function calculateRemaining(startTime: string, durationMinutes: number): number {
  const start = new Date(startTime).getTime();
  const end = start + durationMinutes * 60 * 1000;
  const now = Date.now();
  return Math.max(0, Math.floor((end - now) / 1000));
}
