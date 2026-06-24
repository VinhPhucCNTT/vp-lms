import * as React from "react";
import { SearchIcon, ClockIcon, CheckCircleIcon, AlertCircleIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/shared/components/page-header";
import { assignments, assessments, courses } from "@/shared/data/courses";
import { students } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";
import { cn } from "@/lib/utils";
import type { Assignment, Assessment } from "@/types";

type AssignmentStatus = "pending" | "submitted" | "graded" | "overdue";

interface AssignmentWithStatus extends Assignment {
  course: { code: string; title: string };
  status: AssignmentStatus;
  score?: number;
  submittedAt?: string;
}

interface AssessmentWithStatus extends Assessment {
  course: { code: string; title: string };
  status: "not-started" | "in-progress" | "completed" | "overdue";
  score?: number;
  attemptsUsed: number;
}

export function StudentAssignments() {
  const { user } = useAuth();
  const currentUser = students.find((s) => s.id === user?.id) ?? students[0];
  const [search, setSearch] = React.useState("");
  const [statusFilter, setStatusFilter] = React.useState<string>("all");

  const enrolledCourseIds = currentUser.enrolledCourses;

  const allAssignments: AssignmentWithStatus[] = assignments
    .filter((a) => enrolledCourseIds.some((id) => courses.find((c) => c.id === id)?.id))
    .map((a) => {
      const module = { moduleId: a.moduleId };
      const course = courses.find((c) => c.id === "cs-101")!;
      const statuses: AssignmentStatus[] = ["pending", "submitted", "graded", "overdue"];
      const randomStatus = statuses[Math.floor(Math.random() * statuses.length)];
      return {
        ...a,
        course: { code: course.code, title: course.title },
        status: randomStatus,
        score: randomStatus === "graded" ? Math.floor(Math.random() * 40) + 60 : undefined,
        submittedAt: randomStatus !== "pending" ? "2026-01-15" : undefined,
      };
    });

  const allAssessments: AssessmentWithStatus[] = assessments
    .filter((a) => enrolledCourseIds.some((id) => courses.find((c) => c.id === id)?.id))
    .map((a) => {
      const course = courses.find((c) => c.id === "cs-101")!;
      const statuses: AssessmentWithStatus["status"][] = ["not-started", "in-progress", "completed", "overdue"];
      const randomStatus = statuses[Math.floor(Math.random() * statuses.length)];
      return {
        ...a,
        course: { code: course.code, title: course.title },
        status: randomStatus,
        score: randomStatus === "completed" ? Math.floor(Math.random() * 30) + 70 : undefined,
        attemptsUsed: Math.floor(Math.random() * a.maxAttempts),
      };
    });

  const filteredAssignments = allAssignments.filter((a) => {
    const matchesSearch = a.title.toLowerCase().includes(search.toLowerCase()) || a.course.code.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === "all" || a.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const statusColors: Record<AssignmentStatus, string> = {
    pending: "bg-warning/20 text-warning-foreground",
    submitted: "bg-info/20 text-info",
    graded: "bg-success/20 text-success",
    overdue: "bg-destructive/20 text-destructive",
  };

  const assessmentStatusColors: Record<AssessmentWithStatus["status"], string> = {
    "not-started": "bg-muted text-muted-foreground",
    "in-progress": "bg-info/20 text-info",
    completed: "bg-success/20 text-success",
    overdue: "bg-destructive/20 text-destructive",
  };

  const pendingCount = allAssignments.filter((a) => a.status === "pending" || a.status === "overdue").length;
  const completedCount = allAssignments.filter((a) => a.status === "graded").length;
  const avgScore = allAssignments.filter((a) => a.score).reduce((sum, a) => sum + (a.score || 0), 0) / Math.max(allAssignments.filter((a) => a.score).length, 1);

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
          <CardContent><p className="text-2xl font-bold">{avgScore.toFixed(1)}%</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Progress</CardTitle></CardHeader>
          <CardContent>
            <Progress value={(completedCount / allAssignments.length) * 100} className="mt-2" />
            <p className="text-xs text-muted-foreground mt-1">{completedCount}/{allAssignments.length} completed</p>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="assignments">
        <TabsList>
          <TabsTrigger value="assignments">Assignments ({allAssignments.length})</TabsTrigger>
          <TabsTrigger value="assessments">Assessments ({allAssessments.length})</TabsTrigger>
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

          <div className="space-y-3">
            {filteredAssignments.map((assignment) => (
              <Card key={assignment.id}>
                <CardContent className="p-4">
                  <div className="flex items-start justify-between">
                    <div className="space-y-2">
                      <div className="flex items-center gap-2">
                        <Badge variant="outline">{assignment.course.code}</Badge>
                        <Badge className={cn(statusColors[assignment.status])}>{assignment.status}</Badge>
                      </div>
                      <h3 className="font-semibold">{assignment.title}</h3>
                      <p className="text-sm text-muted-foreground line-clamp-1">{assignment.description}</p>
                      <div className="flex items-center gap-4 text-sm text-muted-foreground">
                        <span className="flex items-center gap-1"><ClockIcon className="size-3" />Due: {assignment.dueDate}</span>
                        <span>Max Score: {assignment.maxScore}</span>
                        <span>Weight: {assignment.weight}%</span>
                      </div>
                    </div>
                    <div className="text-right">
                      {assignment.status === "graded" && assignment.score && (
                        <div className="mb-2">
                          <p className="text-2xl font-bold">{assignment.score}%</p>
                          <p className="text-xs text-muted-foreground">Grade</p>
                        </div>
                      )}
                      <Link to={`/student/courses/${assignment.course.code.toLowerCase().replace(" ", "-")}`}>
                        <Button size="sm" variant={assignment.status === "pending" ? "default" : "outline"}>
                          {assignment.status === "pending" ? "Start" : assignment.status === "submitted" ? "View" : "Review"}
                        </Button>
                      </Link>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="assessments" className="mt-6 space-y-3">
          {allAssessments.map((assessment) => (
            <Card key={assessment.id}>
              <CardContent className="p-4">
                <div className="flex items-start justify-between">
                  <div className="space-y-2">
                    <div className="flex items-center gap-2">
                      <Badge variant="outline">{assessment.course.code}</Badge>
                      <Badge className={cn(assessmentStatusColors[assessment.status])}>{assessment.status}</Badge>
                    </div>
                    <h3 className="font-semibold">{assessment.title}</h3>
                    <div className="flex items-center gap-4 text-sm text-muted-foreground">
                      <span>Duration: {assessment.duration} min</span>
                      <span>Passing: {assessment.passingScore}%</span>
                      <span>Attempts: {assessment.attemptsUsed}/{assessment.maxAttempts}</span>
                    </div>
                  </div>
                  <div className="text-right">
                    {assessment.status === "completed" && assessment.score && (
                      <div className="mb-2">
                        <p className="text-2xl font-bold">{assessment.score}%</p>
                        <p className="text-xs text-muted-foreground">Score</p>
                      </div>
                    )}
                    <Button size="sm" variant={assessment.status === "not-started" ? "default" : "outline"} disabled={assessment.attemptsUsed >= assessment.maxAttempts}>
                      {assessment.status === "not-started" ? "Start Quiz" : assessment.status === "in-progress" ? "Continue" : "Review"}
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </TabsContent>
      </Tabs>
    </div>
  );
}
