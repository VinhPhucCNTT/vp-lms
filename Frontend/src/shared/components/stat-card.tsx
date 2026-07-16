import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface StatCardProps {
  title: string;
  value: string | number;
  description?: string;
  icon?: React.ReactNode;
  variant?: "default" | "success" | "warning" | "info";
}

export function StatCard({ title, value, description, icon, variant = "default" }: StatCardProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{title}</p>
            <p className="text-2xl font-bold">{value}</p>
            {description && <p className="text-xs text-muted-foreground">{description}</p>}
          </div>
          {icon && (
            <div className={cn("size-10 rounded-lg flex items-center justify-center", variant === "success" && "bg-success/10 text-success", variant === "warning" && "bg-warning/10 text-warning-foreground", variant === "info" && "bg-info/10 text-info", variant === "default" && "bg-primary/10 text-primary")}>
              {icon}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
