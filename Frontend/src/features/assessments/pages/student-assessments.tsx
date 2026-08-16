import * as React from "react";
import { Link } from "react-router-dom";
import {
  ClockIcon,
  CheckCircleIcon,
  TrophyIcon,
  AlertTriangleIcon,
  PlayIcon,
  EyeIcon,
  ClipboardCheckIcon,
  CalendarIcon,
  ZapIcon,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { assessmentApi, type StudentAssessmentSummaryDto } from "@/features/assessments/assessment-api";
import { cn } from "@/lib/utils";

type AssessmentStatus = "available" | "in-progress" | "completed" | "overdue" | "upcoming";

const statusConfig: Record<AssessmentStatus, { label: string; color: string; badge: "default" | "secondary" | "destructive" | "success" | "warning" | "outline" | "info" }> = {
  available: { label: "Available", color: "text-info", badge: "info" },
  "in-progress": { label: "In Progress", color: "text-warning-foreground", badge: "warning" },
  completed: { label: "Completed", color: "text-success", badge: "success" },
  overdue: { label: "Overdue", color: "text-destructive", badge: "destructive" },
  upcoming: { label: "Upcoming", color: "text-muted-foreground", badge: "outline" },
};

export function StudentAssessments() {
  const { data: summaries, loading, error, reload } = useApi<StudentAssessmentSummaryDto[]>(
    () => assessmentApi.getStudentAssessments()
  );

  if (loading) return <LoadingState label="Loading assessments..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  const items = summaries ?? [];
  const stats = {
    available: items.filter((a) => a.status === "available").length,
    completed: items.filter((a) => a.status === "completed").length,
    overdue: items.filter((a) => a.status === "overdue").length,
    avgScore: (() => {
      const scored = items.filter((a) => a.bestScore !== null);
      if (scored.length === 0) return 0;
      const pcts = scored.map((a) => (a.bestScore! / a.bestMaxScore) * 100);
      return pcts.reduce((s, p) => s + p, 0) / pcts.length;
    })(),
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="My Assessments"
        description="Take quizzes and exams, and track your performance"
        breadcrumbs={[{ label: "Dashboard", href: "/student" }, { label: "Assessments" }]}
      />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <ClockIcon className="size-4" />Available
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.available}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <CheckCircleIcon className="size-4 text-success" />Completed
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.completed}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <TrophyIcon className="size-4 text-warning" />Average Score
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.avgScore.toFixed(1)}%</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <AlertTriangleIcon className="size-4 text-destructive" />Overdue
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.overdue}</p></CardContent>
        </Card>
      </div>

      {items.length === 0 ? (
        <EmptyState message="No assessments available right now." />
      ) : (
        <div className="grid gap-4">
          {items.map((item) => {
            const cfg = statusConfig[item.status as AssessmentStatus] ?? statusConfig.available;
            const scorePct = item.bestScore !== null
              ? Math.round((item.bestScore / item.bestMaxScore) * 100)
              : null;
            const passed = item.assessment.passingScore !== undefined && scorePct !== null && scorePct >= item.assessment.passingScore;

            return (
              <Card key={item.assessment.id} className={cn(item.status === "overdue" && "border-destructive/50")}>
                <CardContent className="p-4">
                  <div className="flex items-start justify-between gap-4">
                    <div className="space-y-2 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        {item.course && <Badge variant="outline">{item.course.code}</Badge>}
                        <Badge variant={cfg.badge}>{cfg.label}</Badge>
                        {item.status === "completed" && scorePct !== null && (
                          <Badge variant={passed ? "success" : "destructive"}>
                            {passed ? "Passed" : "Failed"}
                          </Badge>
                        )}
                      </div>
                      <h3 className="font-semibold">{item.assessment.title}</h3>
                      <CardDescription className="line-clamp-2">{item.assessment.description}</CardDescription>
                      <div className="flex items-center gap-4 text-sm text-muted-foreground flex-wrap">
                        <span className="flex items-center gap-1">
                          <ClipboardCheckIcon className="size-3" />
                          {item.questionCount} questions
                        </span>
                        <span className="flex items-center gap-1">
                          <ClockIcon className="size-3" />
                          {item.assessment.duration} min
                        </span>
                        <span className="flex items-center gap-1">
                          <TrophyIcon className="size-3" />
                          {item.assessment.passingScore !== undefined ? `Pass: ${item.assessment.passingScore}%` : "Completion tracked"}
                        </span>
                        <span className="flex items-center gap-1">
                          <ZapIcon className="size-3" />
                          {item.attemptsUsed}/{item.assessment.maxAttempts} attempts
                        </span>
                        <span className="flex items-center gap-1">
                          <CalendarIcon className="size-3" />
                          Due {item.assessment.dueDate}
                        </span>
                      </div>
                    </div>

                    <div className="text-right space-y-3 shrink-0">
                      {item.bestScore !== null && (
                        <div>
                          <p className={cn("text-2xl font-bold", passed ? "text-success" : "text-destructive")}>
                            {scorePct}%
                          </p>
                          <p className="text-xs text-muted-foreground">
                            {item.bestScore}/{item.bestMaxScore} pts
                          </p>
                        </div>
                      )}
                      <Button
                        size="sm"
                        variant={item.status === "available" ? "default" : "outline"}
                        disabled={item.status === "upcoming" || item.status === "overdue"}
                        asChild={item.status === "available" || item.status === "in-progress"}
                      >
                        {(item.status === "available" || item.status === "in-progress") ? (
                          <Link to={`/student/assessments/${item.assessment.id}`}>
                            <PlayIcon className="size-3.5 mr-1" />
                            {item.status === "in-progress" ? "Resume" : "Start"}
                          </Link>
                        ) : item.status === "completed" ? (
                          <Link to={`/student/assessments/${item.assessment.id}/results/${item.latestAttemptSqid}`}>
                            <EyeIcon className="size-3.5 mr-1" />
                            Review
                          </Link>
                        ) : item.status === "upcoming" ? (
                          <span>Not Available</span>
                        ) : (
                          <span>Missed</span>
                        )}
                      </Button>
                    </div>
                  </div>

                  {item.status === "completed" && scorePct !== null && (
                    <>
                      <Separator className="my-3" />
                      <div className="flex items-center gap-3">
                        <Progress value={scorePct} className="h-2 flex-1" />
                        <span className="text-xs text-muted-foreground">
                          Best of {item.attemptsUsed} attempt{item.attemptsUsed !== 1 ? "s" : ""}
                        </span>
                      </div>
                    </>
                  )}
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
