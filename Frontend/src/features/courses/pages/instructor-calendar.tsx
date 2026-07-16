import { CalendarIcon } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { Card, CardContent } from "@/components/ui/card";

export function InstructorCalendar() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Calendar"
        description="Your schedule and upcoming deadlines"
        breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Calendar" }]}
      />
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-24 gap-4 text-center">
          <CalendarIcon className="size-12 text-muted-foreground" />
          <div>
            <p className="font-semibold text-lg">Calendar Coming Soon</p>
            <p className="text-sm text-muted-foreground mt-1">This feature is under development.</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
