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
import { assessments, courses } from "@/shared/data/courses";
import { students } from "@/shared/data/users";
import { getAttemptsByStudent, getQuestionsByAssessment } from "@/shared/data/question-bank";
import { useAuth } from "@/features/auth/auth-context";
import { cn } from "@/lib/utils";
import type { Assessment } from "@/types";

type AssessmentStatus = "available" | "in-progress" | "completed" | "overdue" | "upcoming";

function getAssessmentStatus(assessment: Assessment, studentId: string): AssessmentStatus {
  const now = new Date();
  const due = new Date(assessment.dueDate);
  const availableFrom = assessment.availableFrom ? new Date(assessment.availableFrom) : null;
  const availableTo = assessment.availableTo ? new Date(assessment.availableTo) : null;

  const attempts = getAttemptsByStudent(studentId, assessment.id);
  const hasCompleted = attempts.some((a) => a.status === "graded" || a.status === "submitted");
  const hasInProgress = attempts.some((a) => a.status === "in-progress");

  if (hasCompleted) return "completed";
  if (hasInProgress) return "in-progress";
  if (availableFrom && now < availableFrom) return "upcoming";
  if (availableTo && now > availableTo) return "overdue";
  if (now > due) return "overdue";
  return "available";
}

const statusConfig: Record<AssessmentStatus, { label: string; color: string; badge: "default" | "secondary" | "destructive" | "success" | "warning" | "outline" | "info" }> = {
  available: { label: "Available", color: "text-info", badge: "info" },
  "in-progress": { label: "In Progress", color: "text-warning-foreground", badge: "warning" },
  completed: { label: "Completed", color: "text-success", badge: "success" },
  overdue: { label: "Overdue", color: "text-destructive", badge: "destructive" },
  upcoming: { label: "Upcoming", color: "text-muted-foreground", badge: "outline" },
};

export function StudentAssessments() {
  const { user } = useAuth();
  const currentUser = students.find((s) => s.id === user?.id) ?? students[0];

  const userAssessments = assessments
    .filter((a) => a.status === "published")
    .map((a) => {
      const course = courses.find((c) => c.id === "cs-101")!;
      const status = getAssessmentStatus(a, currentUser.id);
      const attempts = getAttemptsByStudent(currentUser.id, a.id);
      const bestAttempt = attempts.reduce(
        (best, curr) =>
          (curr.score ?? 0) > (best?.score ?? 0) ? curr : best,
        attempts[0] ?? null
      );
      const questions = getQuestionsByAssessment(a.id);
      return {
        ...a,
        course: { code: course.code, title: course.title },
        status,
        bestScore: bestAttempt?.score ?? null,
        bestMaxScore: bestAttempt?.maxScore ?? a.totalPoints ?? 0,
        attemptsUsed: attempts.length,
        questionCount: questions.length,
      };
    });

  const stats = {
    available: userAssessments.filter((a) => a.status === "available").length,
    completed: userAssessments.filter((a) => a.status === "completed").length,
    overdue: userAssessments.filter((a) => a.status === "overdue").length,
    avgScore: (() => {
      const scored = userAssessments.filter((a) => a.bestScore !== null);
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

      <div className="grid gap-4">
        {userAssessments.map((assessment) => {
          const cfg = statusConfig[assessment.status];
          const scorePct = assessment.bestScore !== null
            ? Math.round((assessment.bestScore / assessment.bestMaxScore) * 100)
            : null;
          const passed = scorePct !== null && scorePct >= assessment.passingScore;

          return (
            <Card key={assessment.id} className={cn(assessment.status === "overdue" && "border-destructive/50")}>
              <CardContent className="p-4">
                <div className="flex items-start justify-between gap-4">
                  <div className="space-y-2 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <Badge variant="outline">{assessment.course.code}</Badge>
                      <Badge variant={cfg.badge}>{cfg.label}</Badge>
                      {assessment.status === "completed" && scorePct !== null && (
                        <Badge variant={passed ? "success" : "destructive"}>
                          {passed ? "Passed" : "Failed"}
                        </Badge>
                      )}
                    </div>
                    <h3 className="font-semibold">{assessment.title}</h3>
                    <CardDescription className="line-clamp-2">{assessment.description}</CardDescription>
                    <div className="flex items-center gap-4 text-sm text-muted-foreground flex-wrap">
                      <span className="flex items-center gap-1">
                        <ClipboardCheckIcon className="size-3" />
                        {assessment.questionCount} questions
                      </span>
                      <span className="flex items-center gap-1">
                        <ClockIcon className="size-3" />
                        {assessment.duration} min
                      </span>
                      <span className="flex items-center gap-1">
                        <TrophyIcon className="size-3" />
                        Pass: {assessment.passingScore}%
                      </span>
                      <span className="flex items-center gap-1">
                        <ZapIcon className="size-3" />
                        {assessment.attemptsUsed}/{assessment.maxAttempts} attempts
                      </span>
                      <span className="flex items-center gap-1">
                        <CalendarIcon className="size-3" />
                        Due {assessment.dueDate}
                      </span>
                    </div>
                  </div>

                  <div className="text-right space-y-3 shrink-0">
                    {assessment.bestScore !== null && (
                      <div>
                        <p className={cn("text-2xl font-bold", passed ? "text-success" : "text-destructive")}>
                          {scorePct}%
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {assessment.bestScore}/{assessment.bestMaxScore} pts
                        </p>
                      </div>
                    )}
                    <Button
                      size="sm"
                      variant={assessment.status === "available" ? "default" : "outline"}
                      disabled={assessment.status === "upcoming" || assessment.status === "overdue"}
                      asChild={assessment.status === "available" || assessment.status === "in-progress"}
                    >
                      {(assessment.status === "available" || assessment.status === "in-progress") ? (
                        <Link to={`/student/assessments/${assessment.id}`}>
                          <PlayIcon className="size-3.5 mr-1" />
                          {assessment.status === "in-progress" ? "Resume" : "Start"}
                        </Link>
                      ) : assessment.status === "completed" ? (
                        <Link to={`/student/assessments/${assessment.id}/results`}>
                          <EyeIcon className="size-3.5 mr-1" />
                          Review
                        </Link>
                      ) : assessment.status === "upcoming" ? (
                        <span>Not Available</span>
                      ) : (
                        <span>Missed</span>
                      )}
                    </Button>
                  </div>
                </div>

                {assessment.status === "completed" && scorePct !== null && (
                  <>
                    <Separator className="my-3" />
                    <div className="flex items-center gap-3">
                      <Progress value={scorePct} className="h-2 flex-1" />
                      <span className="text-xs text-muted-foreground">
                        Best of {assessment.attemptsUsed} attempt{assessment.attemptsUsed !== 1 ? "s" : ""}
                      </span>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
