import { TrendingUpIcon, TrendingDownIcon } from "lucide-react"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"

interface StatCardProps {
  title: string
  value: string | number
  description?: string
  trend?: { value: number; label: string }
  icon?: React.ReactNode
  variant?: "default" | "success" | "warning" | "info"
}

export function StatCard({
  title,
  value,
  description,
  trend,
  icon,
  variant = "default",
}: StatCardProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{title}</p>
            <p className="text-2xl font-bold">{value}</p>
            {description && (
              <p className="text-xs text-muted-foreground">{description}</p>
            )}
            {trend && (
              <div className="flex items-center gap-1 pt-1">
                {trend.value >= 0 ? (
                  <TrendingUpIcon className="size-3 text-success" />
                ) : (
                  <TrendingDownIcon className="size-3 text-destructive" />
                )}
                <Badge
                  variant={trend.value >= 0 ? "success" : "destructive"}
                  className="text-xs"
                >
                  {trend.value >= 0 ? "+" : ""}
                  {trend.value}%
                </Badge>
                <span className="text-xs text-muted-foreground">
                  {trend.label}
                </span>
              </div>
            )}
          </div>
          {icon && (
            <div
              className={cn(
                "flex size-10 items-center justify-center rounded-lg",
                variant === "success" && "bg-success/10 text-success",
                variant === "warning" &&
                  "bg-warning/10 text-warning-foreground",
                variant === "info" && "bg-info/10 text-info",
                variant === "default" && "bg-primary/10 text-primary"
              )}
            >
              {icon}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  )
}
