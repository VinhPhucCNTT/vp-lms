import { BellIcon } from "lucide-react";
import { PageHeader } from "@/shared/components/page-header";
import { Card, CardContent } from "@/components/ui/card";

export function InstructorNotifications() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Notifications"
        description="Stay up to date with student activity"
        breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Notifications" }]}
      />
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-24 gap-4 text-center">
          <BellIcon className="size-12 text-muted-foreground" />
          <div>
            <p className="font-semibold text-lg">Notifications Coming Soon</p>
            <p className="text-sm text-muted-foreground mt-1">This feature is under development.</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
