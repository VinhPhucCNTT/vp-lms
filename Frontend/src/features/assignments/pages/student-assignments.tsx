import * as React from "react";
import { SearchIcon, ClockIcon, CheckCircleIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { assignmentApi, type AssignmentSummaryDto } from "@/features/assignments/assignment-api";
import { cn } from "@/lib/utils";

type AssignmentStatus = "pending" | "submitted" | "graded" | "overdue";

const statusColors: Record<AssignmentStatus, string> = {
  pending: "bg-warning/20 text-warning-foreground",
  submitted: "bg-info/20 text-info",
  graded: "bg-success/20 text-success",
  overdue: "bg-destructive/20 text-destructive",
};

export function StudentAssignments() {
  const { data: summaries, loading, error, reload } = useApi<AssignmentSummaryDto[]>(
    () => assignmentApi.getStudentAssignments()
  );
  const [search, setSearch] = React.useState("");
  const [statusFilter, setStatusFilter] = React.useState<string>("all");

  const allAssignments = summaries ?? [];

  const filteredAssignments = allAssignments.filter((a) => {
    const matchesSearch = a.assignment.title.toLowerCase().includes(search.toLowerCase()) || (a.course?.code ?? "").toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === "all" || a.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const pendingCount = allAssignments.filter((a) => a.status === "pending" || a.status === "overdue").length;
  const completedCount = allAssignments.filter((a) => a.status === "graded").length;
  const avgScore = allAssignments.filter((a) => a.score).reduce((sum, a) => sum + (a.score || 0), 0) / Math.max(allAssignments.filter((a) => a.score).length, 1);

  if (loading) return <LoadingState label="Loading assignments..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <PageHeader title="My Assignments" description="Track and submit your course assignments" breadcrumbs={[{ label: "Dashboard", href: "/student" }, { label: "Assignments" }]} />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><ClockIcon className="size-4 text-warning" />Pending</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{pendingCount}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><CheckCircleIcon className="size-4 text-success" />Completed</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{completedCount}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Average Score</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{isNaN(avgScore) ? 0 : avgScore.toFixed(1)}%</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Progress</CardTitle></CardHeader>
          <CardContent>
            <Progress value={allAssignments.length > 0 ? (completedCount / allAssignments.length) * 100 : 0} className="mt-2" />
            <p className="text-xs text-muted-foreground mt-1">{completedCount}/{allAssignments.length} completed</p>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="assignments">
        <TabsList>
          <TabsTrigger value="assignments">Assignments ({allAssignments.length})</TabsTrigger>
        </TabsList>

        <TabsContent value="assignments" className="mt-6 space-y-4">
          <div className="flex items-center gap-4">
            <div className="relative flex-1 max-w-md">
              <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
              <Input placeholder="Search assignments..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
            </div>
            <Tabs value={statusFilter} onValueChange={setStatusFilter}>
              <TabsList>
                <TabsTrigger value="all">All</TabsTrigger>
                <TabsTrigger value="pending">Pending</TabsTrigger>
                <TabsTrigger value="submitted">Submitted</TabsTrigger>
                <TabsTrigger value="graded">Graded</TabsTrigger>
              </TabsList>
            </Tabs>
          </div>

          {filteredAssignments.length === 0 ? (
            <EmptyState message="No assignments found." />
          ) : (
            <div className="space-y-3">
              {filteredAssignments.map((item) => {
                const assignment = assignmentApi.mapAssignment(item.assignment);
                const dueDate = item.assignment.info.closeDate
                  ? new Date(item.assignment.info.closeDate).toLocaleDateString()
                  : "No due date";
                return (
                  <Card key={assignment.id}>
                    <CardContent className="p-4">
                      <div className="flex items-start justify-between">
                        <div className="space-y-2">
                          <div className="flex items-center gap-2">
                            <Badge variant="outline">{item.course.code}</Badge>
                            <Badge className={cn(statusColors[item.status as AssignmentStatus] ?? statusColors.pending)}>{item.status}</Badge>
                          </div>
                          <h3 className="font-semibold">{assignment.title}</h3>
                          <p className="text-sm text-muted-foreground line-clamp-1">{assignment.description}</p>
                          <div className="flex items-center gap-4 text-sm text-muted-foreground">
                            <span className="flex items-center gap-1"><ClockIcon className="size-3" />Due: {dueDate}</span>
                            <span>{item.assignment.info.submissionType === 0 || item.assignment.info.submissionType === "File" ? "File submission" : "Text submission"}</span>
                            {item.submittedFileCount > 0 && <span>{item.submittedFileCount} file{item.submittedFileCount === 1 ? "" : "s"}</span>}
                          </div>
                        </div>
                        <div className="text-right">
                          {item.status === "graded" && item.score != null && (
                            <div className="mb-2">
                              <p className="text-2xl font-bold">{item.score}</p>
                              <p className="text-xs text-muted-foreground">Grade</p>
                            </div>
                          )}
                          <Link to={`/student/assignments/${assignment.id}`}>
                            <Button size="sm" variant={item.status === "pending" ? "default" : "outline"}>
                              {item.status === "pending" ? "Start" : item.status === "submitted" ? "View" : "Review"}
                            </Button>
                          </Link>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
