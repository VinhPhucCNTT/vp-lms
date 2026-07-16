import * as React from "react";
import { useParams, Link } from "react-router-dom";
import {
  PlayCircleIcon,
  FileTextIcon,
  BookOpenIcon,
  ClipboardCheckIcon,
  CodeIcon,
  CheckCircle2Icon,
  CircleIcon,
  ClockIcon,
  CalendarIcon,
  AlertCircleIcon,
  ChevronDownIcon,
  ChevronRightIcon,
  MegaphoneIcon,
  UsersIcon,
  BarChart3Icon,
  PaperclipIcon,
  ArrowRightIcon,
  ArrowLeftIcon,
  GraduationCapIcon,
  ZapIcon,
  LockIcon,
  PinIcon,
  ExternalLinkIcon,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import {
  courses,
  modules,
  lessons,
  assignments,
  assessments,
  courseActivities,
} from "@/shared/data/courses";
import { announcements, instructors, students } from "@/shared/data/users";
import { problems } from "@/shared/data/problems";
import type { CourseActivity, ActivityType } from "@/types";
import { useAuth } from "@/features/auth/auth-context";

// ── Icon + colour helpers ────────────────────────────────────────────────────

function activityIcon(type: ActivityType, className?: string) {
  const cls = cn("size-4 shrink-0", className);
  switch (type) {
    case "lesson":       return <PlayCircleIcon className={cls} />;
    case "assignment":   return <FileTextIcon className={cls} />;
    case "assessment":   return <ClipboardCheckIcon className={cls} />;
    case "coding-problem": return <CodeIcon className={cls} />;
  }
}

function activityTypeLabel(type: ActivityType) {
  switch (type) {
    case "lesson":         return "Lesson";
    case "assignment":     return "Assignment";
    case "assessment":     return "Quiz";
    case "coding-problem": return "Coding Problem";
  }
}

function activityTypeBadgeVariant(type: ActivityType) {
  switch (type) {
    case "lesson":         return "secondary" as const;
    case "assignment":     return "info" as const;
    case "assessment":     return "warning" as const;
    case "coding-problem": return "success" as const;
  }
}

// Deterministic "completed" state based on activity id
function isActivityCompleted(activityId: string) {
  const seed = activityId.split("").reduce((acc, c) => acc + c.charCodeAt(0), 0);
  return (seed % 10) > 4;
}

// ── Utility: get rich metadata from a CourseActivity's refId ─────────────────

function getActivityMeta(activity: CourseActivity) {
  switch (activity.type) {
    case "lesson": {
      const l = lessons.find((x) => x.id === activity.refId);
      return l ? { duration: `${l.duration} min`, dueDate: null, maxScore: null, attempts: null } : null;
    }
    case "assignment": {
      const a = assignments.find((x) => x.id === activity.refId);
      return a ? { duration: null, dueDate: a.dueDate, maxScore: a.maxScore, weight: a.weight, attempts: null } : null;
    }
    case "assessment": {
      const a = assessments.find((x) => x.id === activity.refId);
      return a ? { duration: `${a.duration} min`, dueDate: a.dueDate, maxScore: a.passingScore, attempts: a.maxAttempts } : null;
    }
    case "coding-problem": {
      const p = problems.find((x) => x.id === activity.refId);
      return p ? { duration: null, dueDate: "2026-07-15", maxScore: null, difficulty: p.difficulty, attempts: null } : null;
    }
  }
}

// ── Activity Content renderers ────────────────────────────────────────────────

function LessonContent({ refId }: { refId: string }) {
  const lesson = lessons.find((l) => l.id === refId);
  if (!lesson) return null;
  return (
    <div className="space-y-4">
      <div className="prose prose-sm dark:prose-invert max-w-none leading-relaxed"
        dangerouslySetInnerHTML={{ __html: lesson.content }} />
    </div>
  );
}

function AssignmentContent({ refId }: { refId: string }) {
  const assignment = assignments.find((a) => a.id === refId);
  if (!assignment) return null;
  return (
    <div className="space-y-6">
      <div className="rounded-lg border bg-muted/30 p-4">
        <p className="text-sm leading-relaxed">{assignment.description}</p>
      </div>
      <div>
        <h3 className="font-semibold mb-3">Submission</h3>
        <div className="rounded-lg border border-dashed p-8 text-center space-y-3">
          <PaperclipIcon className="size-8 text-muted-foreground mx-auto" />
          <div>
            <p className="font-medium text-sm">Attach your work</p>
            <p className="text-xs text-muted-foreground mt-1">PDF, DOCX, ZIP — up to 25 MB</p>
          </div>
          <Button variant="outline" size="sm">Choose Files</Button>
        </div>
      </div>
      <Button className="w-full sm:w-auto">Submit Assignment</Button>
    </div>
  );
}

function AssessmentContent({ refId }: { refId: string }) {
  const assessment = assessments.find((a) => a.id === refId);
  if (!assessment) return null;
  return (
    <div className="space-y-6">
      <div className="rounded-lg border bg-muted/30 p-4 space-y-3">
        <p className="text-sm">{assessment.description ?? "This quiz tests your knowledge of the module content."}</p>
        <div className="grid grid-cols-2 gap-3 text-sm">
          <div className="flex items-center gap-2 text-muted-foreground">
            <ClockIcon className="size-4" />
            <span>Time limit: <strong className="text-foreground">{assessment.duration} minutes</strong></span>
          </div>
          <div className="flex items-center gap-2 text-muted-foreground">
            <ZapIcon className="size-4" />
            <span>Max attempts: <strong className="text-foreground">{assessment.maxAttempts}</strong></span>
          </div>
          <div className="flex items-center gap-2 text-muted-foreground">
            <GraduationCapIcon className="size-4" />
            <span>Passing score: <strong className="text-foreground">{assessment.passingScore}%</strong></span>
          </div>
        </div>
      </div>
      <div className="rounded-lg border bg-amber-50 dark:bg-amber-950/20 border-amber-200 dark:border-amber-900 p-4">
        <p className="text-sm text-amber-800 dark:text-amber-200 font-medium">Once started, the timer cannot be paused.</p>
      </div>
      <Button size="lg">Begin Quiz</Button>
    </div>
  );
}

function CodingProblemContent({ refId, courseId }: { refId: string; courseId: string }) {
  const problem = problems.find((p) => p.id === refId);
  if (!problem) return null;
  const acceptanceRate = Math.round((problem.acceptedCount / problem.submissionCount) * 100);
  return (
    <div className="space-y-6">
      <div className="prose prose-sm dark:prose-invert max-w-none">
        <p>{problem.description}</p>
      </div>
      {problem.examples.length > 0 && (
        <div className="space-y-3">
          <h3 className="font-semibold">Examples</h3>
          {problem.examples.map((ex, i) => (
            <div key={i} className="rounded-lg border bg-muted/40 p-4 font-mono text-sm space-y-1">
              <p><span className="text-muted-foreground">Input:</span> {ex.input}</p>
              <p><span className="text-muted-foreground">Output:</span> {ex.output}</p>
              {ex.explanation && <p className="text-muted-foreground text-xs pt-1">{ex.explanation}</p>}
            </div>
          ))}
        </div>
      )}
      <div className="flex items-center gap-3 text-sm text-muted-foreground">
        <span>Acceptance rate: <strong className="text-foreground">{acceptanceRate}%</strong></span>
        <span className="text-border">·</span>
        <span>Time limit: <strong className="text-foreground">{problem.timeLimit}ms</strong></span>
        <span className="text-border">·</span>
        <span>Memory: <strong className="text-foreground">{problem.memoryLimit}MB</strong></span>
      </div>
      <Button asChild size="lg">
        <Link to={`/student/courses/${courseId}/problems/${problem.id}`}>
          Open in Code Editor <ExternalLinkIcon className="size-4 ml-2" />
        </Link>
      </Button>
    </div>
  );
}

// ── People tab ────────────────────────────────────────────────────────────────

function PeopleTab({ courseId }: { courseId: string }) {
  const course = courses.find((c) => c.id === courseId)!;
  const instructor = instructors.find((i) => i.id === course.instructorId);
  const enrolled = students.filter((s) => s.enrolledCourses.includes(courseId));

  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-base font-semibold mb-3">Instructor</h2>
        {instructor && (
          <div className="flex items-center gap-4 p-4 rounded-lg border bg-card">
            <div className="size-12 rounded-full bg-primary/10 flex items-center justify-center font-semibold text-primary">
              {instructor.firstName[0]}{instructor.lastName[0]}
            </div>
            <div>
              <p className="font-semibold">{instructor.firstName} {instructor.lastName}</p>
              <p className="text-sm text-muted-foreground">{instructor.department}</p>
              {instructor.officeLocation && (
                <p className="text-xs text-muted-foreground mt-0.5">{instructor.officeLocation}</p>
              )}
            </div>
          </div>
        )}
      </div>
      <div>
        <h2 className="text-base font-semibold mb-3">Students <span className="text-muted-foreground font-normal">({enrolled.length})</span></h2>
        <div className="grid gap-2 sm:grid-cols-2">
          {enrolled.map((s) => (
            <div key={s.id} className="flex items-center gap-3 p-3 rounded-lg border bg-card">
              <div className="size-8 rounded-full bg-muted flex items-center justify-center text-xs font-semibold text-muted-foreground">
                {s.firstName[0]}{s.lastName[0]}
              </div>
              <div>
                <p className="text-sm font-medium">{s.firstName} {s.lastName}</p>
                <p className="text-xs text-muted-foreground">{s.studentId}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

// ── Grades tab ────────────────────────────────────────────────────────────────

function GradesTab({ courseId }: { courseId: string }) {
  const activities = courseActivities.filter((a) => a.courseId === courseId && a.type !== "lesson");
  const graded = activities.map((a) => {
    const seed = a.id.split("").reduce((acc, c) => acc + c.charCodeAt(0), 0);
    const completed = (seed % 10) > 4;
    let maxScore = 100;
    let earnedScore: number | null = completed ? Math.round(60 + (seed % 40)) : null;
    if (a.type === "assignment") {
      const asgn = assignments.find((x) => x.id === a.refId);
      if (asgn) maxScore = asgn.maxScore;
    }
    return { activity: a, maxScore, earnedScore };
  });

  const totalEarned = graded.reduce((s, g) => s + (g.earnedScore ?? 0), 0);
  const totalMax = graded.reduce((s, g) => s + g.maxScore, 0);
  const overallPct = totalMax > 0 ? Math.round((totalEarned / totalMax) * 100) : 0;

  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold">{overallPct}%</p>
            <p className="text-sm text-muted-foreground mt-1">Overall Grade</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold">{graded.filter((g) => g.earnedScore !== null).length}</p>
            <p className="text-sm text-muted-foreground mt-1">Graded Items</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6 text-center">
            <p className="text-3xl font-bold">{graded.filter((g) => g.earnedScore === null).length}</p>
            <p className="text-sm text-muted-foreground mt-1">Pending</p>
          </CardContent>
        </Card>
      </div>

      <div className="rounded-lg border overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-muted/50">
            <tr>
              <th className="text-left px-4 py-3 font-medium">Activity</th>
              <th className="text-left px-4 py-3 font-medium">Type</th>
              <th className="text-right px-4 py-3 font-medium">Score</th>
            </tr>
          </thead>
          <tbody>
            {graded.map(({ activity, maxScore, earnedScore }, i) => (
              <tr key={activity.id} className={cn("border-t", i % 2 === 1 && "bg-muted/20")}>
                <td className="px-4 py-3">{activity.title}</td>
                <td className="px-4 py-3">
                  <Badge variant={activityTypeBadgeVariant(activity.type)} className="text-xs">
                    {activityTypeLabel(activity.type)}
                  </Badge>
                </td>
                <td className="px-4 py-3 text-right">
                  {earnedScore !== null
                    ? <span className="font-semibold">{earnedScore} / {maxScore}</span>
                    : <span className="text-muted-foreground">—</span>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// ── Progress tab ──────────────────────────────────────────────────────────────

function ProgressTab({ courseId }: { courseId: string }) {
  const courseModules = modules.filter((m) => m.courseId === courseId);
  const allActivities = courseActivities.filter((a) => a.courseId === courseId);
  const completed = allActivities.filter((a) => isActivityCompleted(a.id)).length;
  const total = allActivities.length;
  const pct = total > 0 ? Math.round((completed / total) * 100) : 0;

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Overall Progress</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
          <div className="flex items-end justify-between text-sm">
            <span className="text-muted-foreground">{completed} of {total} activities complete</span>
            <span className="text-2xl font-bold">{pct}%</span>
          </div>
          <Progress value={pct} className="h-3" />
        </CardContent>
      </Card>

      <div className="space-y-4">
        {courseModules.map((mod) => {
          const modActivities = allActivities.filter((a) => a.moduleId === mod.id);
          const modCompleted = modActivities.filter((a) => isActivityCompleted(a.id)).length;
          const modPct = modActivities.length > 0 ? Math.round((modCompleted / modActivities.length) * 100) : 0;
          return (
            <div key={mod.id} className="rounded-lg border bg-card p-4 space-y-3">
              <div className="flex items-center justify-between">
                <p className="font-medium text-sm">{mod.title}</p>
                <span className="text-xs text-muted-foreground">{modCompleted}/{modActivities.length}</span>
              </div>
              <Progress value={modPct} className="h-2" />
              <div className="flex flex-wrap gap-1.5">
                {modActivities.map((a) => {
                  const done = isActivityCompleted(a.id);
                  return (
                    <div key={a.id} title={a.title}
                      className={cn("size-6 rounded flex items-center justify-center",
                        done ? "bg-primary/15 text-primary" : "bg-muted text-muted-foreground")}>
                      {activityIcon(a.type, cn("size-3", done ? "text-primary" : ""))}
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ── Announcements tab ─────────────────────────────────────────────────────────

function AnnouncementsTab({ courseId }: { courseId: string }) {
  const courseAnnouncements = announcements.filter((a) => a.courseId === courseId);
  return (
    <div className="space-y-4">
      {courseAnnouncements.length === 0 && (
        <div className="py-16 text-center text-muted-foreground">No announcements yet.</div>
      )}
      {courseAnnouncements.map((ann) => (
        <Card key={ann.id}>
          <CardHeader className="pb-2">
            <div className="flex items-start gap-2">
              {ann.isPinned && (
                <PinIcon className="size-4 text-primary mt-0.5 shrink-0" />
              )}
              <div className="space-y-1">
                <CardTitle className="text-base">{ann.title}</CardTitle>
                <CardDescription>{ann.createdAt}</CardDescription>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <p className="text-sm leading-relaxed">{ann.content}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

// ── Main activity pane ────────────────────────────────────────────────────────

function ActivityPane({
  activity,
  courseId,
  allFlatActivities,
  onNavigate,
}: {
  activity: CourseActivity;
  courseId: string;
  allFlatActivities: CourseActivity[];
  onNavigate: (id: string) => void;
}) {
  const meta = getActivityMeta(activity);
  const completed = isActivityCompleted(activity.id);
  const currentIndex = allFlatActivities.findIndex((a) => a.id === activity.id);
  const prev = currentIndex > 0 ? allFlatActivities[currentIndex - 1] : null;
  const next = currentIndex < allFlatActivities.length - 1 ? allFlatActivities[currentIndex + 1] : null;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="space-y-3">
        <div className="flex items-center gap-2 flex-wrap">
          <Badge variant={activityTypeBadgeVariant(activity.type)} className="gap-1.5">
            {activityIcon(activity.type, "size-3")}
            {activityTypeLabel(activity.type)}
          </Badge>
          {completed ? (
            <Badge variant="success" className="gap-1.5">
              <CheckCircle2Icon className="size-3" />
              Completed
            </Badge>
          ) : (
            <Badge variant="outline" className="gap-1.5 text-muted-foreground">
              <CircleIcon className="size-3" />
              Not started
            </Badge>
          )}
        </div>
        <h1 className="text-2xl font-bold tracking-tight">{activity.title}</h1>

        {/* Metadata strip */}
        {meta && (
          <div className="flex items-center gap-4 flex-wrap text-sm text-muted-foreground border-b pb-4">
            {meta.duration && (
              <span className="flex items-center gap-1.5">
                <ClockIcon className="size-4" />
                {meta.duration}
              </span>
            )}
            {meta.dueDate && (
              <span className="flex items-center gap-1.5">
                <CalendarIcon className="size-4" />
                Due {meta.dueDate}
              </span>
            )}
            {meta.maxScore && (
              <span className="flex items-center gap-1.5">
                <GraduationCapIcon className="size-4" />
                {activity.type === "assessment" ? `Passing: ${meta.maxScore}%` : `${meta.maxScore} pts`}
              </span>
            )}
            {(meta as any).weight && (
              <span className="flex items-center gap-1.5">
                <BarChart3Icon className="size-4" />
                {(meta as any).weight}% of grade
              </span>
            )}
            {(meta as any).difficulty && (
              <Badge className={cn("text-xs",
                (meta as any).difficulty === "easy" && "bg-success/10 text-success border-success/20",
                (meta as any).difficulty === "medium" && "bg-warning/10 text-warning-foreground border-warning/20",
                (meta as any).difficulty === "hard" && "bg-destructive/10 text-destructive border-destructive/20",
              )} variant="outline">
                {(meta as any).difficulty}
              </Badge>
            )}
          </div>
        )}
      </div>

      {/* Content */}
      {activity.type === "lesson" && <LessonContent refId={activity.refId} />}
      {activity.type === "assignment" && <AssignmentContent refId={activity.refId} />}
      {activity.type === "assessment" && <AssessmentContent refId={activity.refId} />}
      {activity.type === "coding-problem" && <CodingProblemContent refId={activity.refId} courseId={courseId} />}

      {/* Navigation footer */}
      <Separator className="mt-8" />
      <div className="flex items-center justify-between pt-2 pb-6">
        <Button
          variant="outline"
          disabled={!prev}
          onClick={() => prev && onNavigate(prev.id)}
          className="gap-2"
        >
          <ArrowLeftIcon className="size-4" />
          {prev ? prev.title : "Previous"}
        </Button>

        {!completed && (
          <Button variant="default" className="gap-2">
            <CheckCircle2Icon className="size-4" />
            Mark as Complete
          </Button>
        )}

        <Button
          variant={next ? "default" : "outline"}
          disabled={!next}
          onClick={() => next && onNavigate(next.id)}
          className="gap-2"
        >
          {next ? next.title : "Next"}
          <ArrowRightIcon className="size-4" />
        </Button>
      </div>
    </div>
  );
}

// ── Sidebar ───────────────────────────────────────────────────────────────────

function CourseSidebar({
  courseId,
  selectedActivityId,
  onSelect,
}: {
  courseId: string;
  selectedActivityId: string;
  onSelect: (id: string) => void;
}) {
  const courseModules = modules.filter((m) => m.courseId === courseId);
  const allActivities = courseActivities.filter((a) => a.courseId === courseId);

  const [expandedModules, setExpandedModules] = React.useState<Set<string>>(
    () => new Set(courseModules.map((m) => m.id))
  );

  const toggleModule = (id: string) => {
    setExpandedModules((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const totalActivities = allActivities.length;
  const completedCount = allActivities.filter((a) => isActivityCompleted(a.id)).length;
  const overallPct = totalActivities > 0 ? Math.round((completedCount / totalActivities) * 100) : 0;

  return (
    <div className="flex flex-col h-full">
      {/* Course progress summary */}
      <div className="p-4 border-b space-y-2">
        <div className="flex items-center justify-between text-sm">
          <span className="font-medium text-foreground">Course Progress</span>
          <span className="font-bold">{overallPct}%</span>
        </div>
        <Progress value={overallPct} className="h-2" />
        <p className="text-xs text-muted-foreground">{completedCount} of {totalActivities} completed</p>
      </div>

      <ScrollArea className="flex-1">
        <div className="p-2 space-y-1">
          {courseModules.map((mod) => {
            const modActivities = allActivities
              .filter((a) => a.moduleId === mod.id)
              .sort((a, b) => a.order - b.order);
            const modCompleted = modActivities.filter((a) => isActivityCompleted(a.id)).length;
            const isExpanded = expandedModules.has(mod.id);
            const modPct = modActivities.length > 0
              ? Math.round((modCompleted / modActivities.length) * 100)
              : 0;

            return (
              <div key={mod.id}>
                {/* Module header */}
                <button
                  onClick={() => toggleModule(mod.id)}
                  className="w-full text-left px-3 py-2.5 rounded-lg hover:bg-sidebar-accent hover:text-sidebar-accent-foreground transition-colors group"
                >
                  <div className="flex items-center gap-2">
                    {isExpanded
                      ? <ChevronDownIcon className="size-4 text-muted-foreground shrink-0" />
                      : <ChevronRightIcon className="size-4 text-muted-foreground shrink-0" />}
                    <span className="text-sm font-semibold leading-tight flex-1 text-left">{mod.title}</span>
                  </div>
                  <div className="flex items-center gap-2 ml-6 mt-1.5">
                    <Progress value={modPct} className="h-1 flex-1" />
                    <span className="text-xs text-muted-foreground shrink-0">{modCompleted}/{modActivities.length}</span>
                  </div>
                </button>

                {/* Activities */}
                {isExpanded && (
                  <div className="ml-2 mt-0.5 space-y-0.5">
                    {modActivities.map((activity) => {
                      const done = isActivityCompleted(activity.id);
                      const isSelected = activity.id === selectedActivityId;
                      const meta = getActivityMeta(activity);
                      const dueDate = meta?.dueDate;
                      const isPastDue = dueDate && new Date(dueDate) < new Date() && !done;

                      return (
                        <button
                          key={activity.id}
                          onClick={() => onSelect(activity.id)}
                          className={cn(
                            "w-full text-left px-3 py-2 rounded-lg transition-colors group flex items-start gap-2.5",
                            isSelected
                              ? "bg-sidebar-primary text-sidebar-primary-foreground"
                              : "hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                          )}
                        >
                          {/* Completion indicator */}
                          <div className="mt-0.5 shrink-0">
                            {done ? (
                              <CheckCircle2Icon className={cn("size-4", isSelected ? "text-sidebar-primary-foreground" : "text-primary")} />
                            ) : (
                              <CircleIcon className={cn("size-4", isSelected ? "text-sidebar-primary-foreground/60" : "text-muted-foreground")} />
                            )}
                          </div>

                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-1.5">
                              {activityIcon(activity.type, cn("shrink-0",
                                isSelected ? "text-sidebar-primary-foreground/80" : "text-muted-foreground"
                              ))}
                              <span className={cn("text-sm leading-tight truncate",
                                isSelected ? "font-semibold" : "font-medium"
                              )}>
                                {activity.title}
                              </span>
                            </div>
                            {dueDate && !done && (
                              <p className={cn(
                                "text-xs mt-0.5 flex items-center gap-1",
                                isPastDue
                                  ? (isSelected ? "text-red-300" : "text-destructive")
                                  : (isSelected ? "text-sidebar-primary-foreground/60" : "text-muted-foreground")
                              )}>
                                {isPastDue && <AlertCircleIcon className="size-3" />}
                                {isPastDue ? "Overdue" : `Due ${dueDate}`}
                              </p>
                            )}
                          </div>
                        </button>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </ScrollArea>
    </div>
  );
}

// ── Root component ────────────────────────────────────────────────────────────

export function CourseWorkspace() {
  const { courseId } = useParams<{ courseId: string }>();
  const { user } = useAuth();
  const course = courses.find((c) => c.id === courseId) ?? courses[0];
  const instructor = instructors.find((i) => i.id === course.instructorId);
  const courseAnnouncements = announcements.filter((a) => a.courseId === course.id);

  // Flat ordered list of all activities across all modules for prev/next nav
  const courseModules = modules.filter((m) => m.courseId === course.id).sort((a, b) => a.order - b.order);
  const allFlatActivities = courseModules.flatMap((mod) =>
    courseActivities
      .filter((a) => a.moduleId === mod.id)
      .sort((a, b) => a.order - b.order)
  );

  const [selectedActivityId, setSelectedActivityId] = React.useState<string>(
    allFlatActivities[0]?.id ?? ""
  );
  const [topTab, setTopTab] = React.useState("learn");

  const selectedActivity = allFlatActivities.find((a) => a.id === selectedActivityId);

  const totalActivities = allFlatActivities.length;
  const completedCount = allFlatActivities.filter((a) => isActivityCompleted(a.id)).length;
  const overallPct = totalActivities > 0 ? Math.round((completedCount / totalActivities) * 100) : 0;

  return (
    <div className="h-[calc(100vh-3.5rem)] flex flex-col overflow-hidden">
      {/* ── Course header bar ─────────────────────────────────────────── */}
      <div className="border-b bg-card shrink-0">
        <div className="px-4 lg:px-6 py-3 flex items-center justify-between gap-4">
          <div className="flex items-center gap-3 min-w-0">
            <Link to="/student/courses" className="text-muted-foreground hover:text-foreground transition-colors shrink-0">
              <ArrowLeftIcon className="size-4" />
            </Link>
            <div className="min-w-0">
              <div className="flex items-center gap-2">
                <Badge variant="outline" className="shrink-0">{course.code}</Badge>
                <h1 className="text-base font-bold truncate">{course.title}</h1>
              </div>
              {instructor && (
                <p className="text-xs text-muted-foreground mt-0.5">
                  {instructor.firstName} {instructor.lastName} · {course.semester}
                </p>
              )}
            </div>
          </div>
          <div className="hidden sm:flex items-center gap-3 shrink-0">
            <div className="text-right">
              <p className="text-xs text-muted-foreground">Progress</p>
              <p className="text-sm font-bold">{overallPct}%</p>
            </div>
            <Progress value={overallPct} className="w-24 h-2" />
          </div>
        </div>

        {/* ── Top navigation tabs ──────────────────────────────────────── */}
        <Tabs value={topTab} onValueChange={setTopTab}>
          <TabsList variant="line" className="px-4 lg:px-6 h-auto pb-0 bg-transparent rounded-none border-0 gap-0">
            <TabsTrigger value="learn" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 pb-2.5">
              <BookOpenIcon className="size-4 mr-2" />Learn
            </TabsTrigger>
            <TabsTrigger value="announcements" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 pb-2.5">
              <MegaphoneIcon className="size-4 mr-2" />Announcements
              {courseAnnouncements.length > 0 && (
                <Badge variant="destructive" className="ml-2 text-xs h-4 px-1">{courseAnnouncements.length}</Badge>
              )}
            </TabsTrigger>
            <TabsTrigger value="people" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 pb-2.5">
              <UsersIcon className="size-4 mr-2" />People
            </TabsTrigger>
            <TabsTrigger value="grades" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 pb-2.5">
              <GraduationCapIcon className="size-4 mr-2" />Grades
            </TabsTrigger>
            <TabsTrigger value="progress" className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 pb-2.5">
              <BarChart3Icon className="size-4 mr-2" />Progress
            </TabsTrigger>
          </TabsList>
        </Tabs>
      </div>

      {/* ── Body ──────────────────────────────────────────────────────── */}
      <div className="flex-1 overflow-hidden flex">
        {topTab === "learn" ? (
          <>
            {/* Left sidebar: module hierarchy */}
            <div className="w-72 shrink-0 border-r bg-sidebar flex flex-col overflow-hidden">
              <CourseSidebar
                courseId={course.id}
                selectedActivityId={selectedActivityId}
                onSelect={setSelectedActivityId}
              />
            </div>

            {/* Main content pane */}
            <ScrollArea className="flex-1">
              <div className="max-w-3xl mx-auto px-6 lg:px-10 py-8">
                {selectedActivity ? (
                  <ActivityPane
                    activity={selectedActivity}
                    courseId={course.id}
                    allFlatActivities={allFlatActivities}
                    onNavigate={setSelectedActivityId}
                  />
                ) : (
                  <div className="text-center py-24 text-muted-foreground">
                    Select an activity from the sidebar to get started.
                  </div>
                )}
              </div>
            </ScrollArea>
          </>
        ) : (
          <ScrollArea className="flex-1">
            <div className="max-w-3xl mx-auto px-6 lg:px-10 py-8">
              {topTab === "announcements" && <AnnouncementsTab courseId={course.id} />}
              {topTab === "people" && <PeopleTab courseId={course.id} />}
              {topTab === "grades" && <GradesTab courseId={course.id} />}
              {topTab === "progress" && <ProgressTab courseId={course.id} />}
            </div>
          </ScrollArea>
        )}
      </div>
    </div>
  );
}
