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
    ChevronRightIcon,
    ChevronLeftIcon,
    MegaphoneIcon,
    UsersIcon,
    BarChart3Icon,
    PaperclipIcon,
    ArrowRightIcon,
    ArrowLeftIcon,
    GraduationCapIcon,
    ZapIcon,
    PinIcon,
    ExternalLinkIcon,
    SearchIcon,
    LayersIcon,
    CircleDotIcon,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Input } from "@/components/ui/input";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import { useApi } from "@/lib/use-api";
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import { courseApi } from "@/features/courses/api/course-api";
import type { ModuleResponse } from "@/features/courses/api/course-api";
import { ActivityType as ApiActivityType, type ActivityResponse } from "@/features/activity/activity-api";
import type { PaginatedResponse } from "@/shared/types";
import { ApiError } from "@/lib/api-client";
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
import type { CourseActivity, ActivityType, Module } from "@/types";

// ── Icon + colour helpers ────────────────────────────────────────────────────

function activityIcon(type: ActivityType, className?: string) {
    const cls = cn("size-4 shrink-0", className);
    switch (type) {
        case "lesson": return <PlayCircleIcon className={cls} />;
        case "assignment": return <FileTextIcon className={cls} />;
        case "assessment": return <ClipboardCheckIcon className={cls} />;
        case "coding-problem": return <CodeIcon className={cls} />;
    }
}

function activityTypeLabel(type: ActivityType) {
    switch (type) {
        case "lesson": return "Lesson";
        case "assignment": return "Assignment";
        case "assessment": return "Quiz";
        case "coding-problem": return "Coding Problem";
    }
}

function activityTypeBadgeVariant(type: ActivityType) {
    switch (type) {
        case "lesson": return "secondary" as const;
        case "assignment": return "info" as const;
        case "assessment": return "warning" as const;
        case "coding-problem": return "success" as const;
    }
}

// Deterministic "completed" state based on activity id
function isActivityCompleted(activityId: string) {
    const seed = activityId.split("").reduce((acc, c) => acc + c.charCodeAt(0), 0);
    return (seed % 10) > 4;
}

// ── Utility: get rich metadata from a CourseActivity's refId ─────────────────

type ActivityMeta = {
    duration: string | null;
    dueDate: string | null;
    maxScore: number | null;
    weight?: number | null;
    attempts?: number | null;
    difficulty?: string | null;
};

function getActivityMeta(activity: CourseActivity): ActivityMeta | null {
    switch (activity.type) {
        case "lesson": {
            const l = lessons.find((x) => x.id === activity.refId);
            return l ? { duration: `${l.duration} min`, dueDate: null, maxScore: null } : null;
        }
        case "assignment": {
            const a = assignments.find((x) => x.id === activity.refId);
            return a ? { duration: null, dueDate: a.dueDate, maxScore: a.maxScore, weight: a.weight } : null;
        }
        case "assessment": {
            const a = assessments.find((x) => x.id === activity.refId);
            return a ? { duration: `${a.duration} min`, dueDate: a.dueDate, maxScore: a.passingScore, attempts: a.maxAttempts } : null;
        }
        case "coding-problem": {
            const p = problems.find((x) => x.id === activity.refId);
            return p ? { duration: null, dueDate: "2026-07-15", maxScore: null, difficulty: p.difficulty } : null;
        }
    }
}

function moduleCompletion(moduleId: string): { completed: number; total: number; pct: number } {
    const modActivities = courseActivities.filter((a) => a.moduleId === moduleId);
    const completed = modActivities.filter((a) => isActivityCompleted(a.id)).length;
    const total = modActivities.length;
    const pct = total > 0 ? Math.round((completed / total) * 100) : 0;
    return { completed, total, pct };
}

function courseCompletion(courseId: string): { completed: number; total: number; pct: number } {
    const all = courseActivities.filter((a) => a.courseId === courseId);
    const completed = all.filter((a) => isActivityCompleted(a.id)).length;
    const total = all.length;
    const pct = total > 0 ? Math.round((completed / total) * 100) : 0;
    return { completed, total, pct };
}

// ── Activity Content renderers ────────────────────────────────────────────────

function LessonContent({ refId }: { refId: string }) {
    const lesson = lessons.find((l) => l.id === refId);
    if (!lesson) return null;
    return (
        <div className="prose prose-sm dark:prose-invert max-w-none leading-relaxed"
            dangerouslySetInnerHTML={{ __html: lesson.content }} />
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
            <Button asChild size="lg">
                <Link to={`/student/assessments/${assessment.id}`}>Begin Quiz</Link>
            </Button>
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
            <div className="flex items-center gap-3 text-sm text-muted-foreground flex-wrap">
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

// ── Breadcrumb ────────────────────────────────────────────────────────────────

function Breadcrumb({ items }: { items: { label: string; onClick?: () => void }[] }) {
    return (
        <nav className="flex items-center gap-1.5 text-sm text-muted-foreground flex-wrap">
            {items.map((item, i) => (
                <React.Fragment key={i}>
                    {i > 0 && <ChevronRightIcon className="size-3.5 text-muted-foreground/50" />}
                    {item.onClick ? (
                        <button
                            onClick={item.onClick}
                            className="hover:text-foreground transition-colors truncate max-w-[200px]"
                        >
                            {item.label}
                        </button>
                    ) : (
                        <span className="text-foreground font-medium truncate max-w-[240px]">{item.label}</span>
                    )}
                </React.Fragment>
            ))}
        </nav>
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
                    const { completed: mc, total: mt, pct: mp } = moduleCompletion(mod.id);
                    const modActivities = allActivities.filter((a) => a.moduleId === mod.id);
                    return (
                        <div key={mod.id} className="rounded-lg border bg-card p-4 space-y-3">
                            <div className="flex items-center justify-between">
                                <p className="font-medium text-sm">{mod.title}</p>
                                <span className="text-xs text-muted-foreground">{mc}/{mt}</span>
                            </div>
                            <Progress value={mp} className="h-2" />
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

// ── Level 1: Module List Page ─────────────────────────────────────────────────

type ModuleFilter = "all" | "in-progress" | "not-started" | "completed";

function ModuleListPage({
    courseId,
    onOpenModule,
}: {
    courseId: string;
    onOpenModule: (moduleId: string) => void;
}) {
    const course = courses.find((c) => c.id === courseId)!;
    const courseModules = modules.filter((m) => m.courseId === courseId).sort((a, b) => a.order - b.order);
    const [search, setSearch] = React.useState("");
    const [filter, setFilter] = React.useState<ModuleFilter>("all");

    const filteredModules = courseModules.filter((mod) => {
        if (search && !mod.title.toLowerCase().includes(search.toLowerCase())) return false;
        const { completed, total, pct } = moduleCompletion(mod.id);
        if (filter === "completed" && pct !== 100) return false;
        if (filter === "not-started" && completed > 0) return false;
        if (filter === "in-progress" && (completed === 0 || pct === 100)) return false;
        return true;
    });

    const { completed: cc, total: ct, pct: cp } = courseCompletion(courseId);

    const filterTabs: { value: ModuleFilter; label: string }[] = [
        { value: "all", label: "All" },
        { value: "in-progress", label: "In Progress" },
        { value: "not-started", label: "Not Started" },
        { value: "completed", label: "Completed" },
    ];

    return (
        <div className="space-y-6">
            {/* Course header */}
            <div className="space-y-4">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight">{course.title}</h1>
                    <p className="text-muted-foreground mt-1 text-sm leading-relaxed max-w-2xl">{course.description}</p>
                </div>
                <div className="flex items-center gap-4 flex-wrap">
                    <div className="flex items-center gap-3">
                        <div className="text-sm">
                            <span className="font-bold text-lg">{cp}%</span>
                            <span className="text-muted-foreground ml-2">{cc} of {ct} activities</span>
                        </div>
                        <Progress value={cp} className="w-32 h-2" />
                    </div>
                    <Badge variant="outline">{courseModules.length} modules</Badge>
                </div>
            </div>

            <Separator />

            {/* Filters */}
            <div className="flex items-center justify-between gap-4 flex-wrap">
                <div className="relative w-full sm:w-72">
                    <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
                    <Input
                        placeholder="Search modules..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className="pl-9"
                    />
                </div>
                <div className="flex items-center gap-1 rounded-lg bg-muted p-1">
                    {filterTabs.map((tab) => (
                        <button
                            key={tab.value}
                            onClick={() => setFilter(tab.value)}
                            className={cn(
                                "px-3 py-1.5 text-sm font-medium rounded-md transition-colors",
                                filter === tab.value
                                    ? "bg-background text-foreground shadow-sm"
                                    : "text-muted-foreground hover:text-foreground"
                            )}
                        >
                            {tab.label}
                        </button>
                    ))}
                </div>
            </div>

            {/* Module cards */}
            {filteredModules.length === 0 ? (
                <div className="py-24 text-center">
                    <LayersIcon className="size-10 text-muted-foreground/40 mx-auto mb-3" />
                    <p className="text-muted-foreground">No modules match your filters.</p>
                </div>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2">
                    {filteredModules.map((mod) => {
                        const { completed, total, pct } = moduleCompletion(mod.id);
                        const modActivities = courseActivities.filter((a) => a.moduleId === mod.id);
                        const allDone = pct === 100;
                        const noneDone = completed === 0;

                        return (
                            <button
                                key={mod.id}
                                onClick={() => onOpenModule(mod.id)}
                                className="group text-left rounded-lg border bg-card p-5 space-y-4 transition-all hover:border-primary/40 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-ring"
                            >
                                <div className="flex items-start justify-between gap-3">
                                    <div className="space-y-1 min-w-0">
                                        <div className="flex items-center gap-2">
                                            <span className="text-xs font-medium text-muted-foreground">Module {mod.order}</span>
                                            {allDone && (
                                                <Badge variant="success" className="text-xs gap-1">
                                                    <CheckCircle2Icon className="size-3" />
                                                    Completed
                                                </Badge>
                                            )}
                                        </div>
                                        <h3 className="font-semibold leading-tight">{mod.title}</h3>
                                    </div>
                                    <ChevronRightIcon className="size-5 text-muted-foreground shrink-0 group-hover:text-foreground group-hover:translate-x-0.5 transition-all" />
                                </div>

                                <div className="space-y-2">
                                    <div className="flex items-center justify-between text-xs text-muted-foreground">
                                        <span>{total} activities · {completed} completed</span>
                                        <span className="font-semibold text-foreground">{pct}%</span>
                                    </div>
                                    <Progress value={pct} className="h-1.5" />
                                </div>

                                {/* Activity type icons summary */}
                                <div className="flex items-center gap-1.5 pt-1">
                                    {modActivities.slice(0, 6).map((a) => {
                                        const done = isActivityCompleted(a.id);
                                        return (
                                            <div
                                                key={a.id}
                                                title={a.title}
                                                className={cn(
                                                    "size-6 rounded flex items-center justify-center",
                                                    done ? "bg-primary/15 text-primary" : "bg-muted text-muted-foreground"
                                                )}
                                            >
                                                {activityIcon(a.type, "size-3")}
                                            </div>
                                        );
                                    })}
                                    {modActivities.length > 6 && (
                                        <span className="text-xs text-muted-foreground ml-1">+{modActivities.length - 6}</span>
                                    )}
                                    <span className="ml-auto text-xs font-medium text-primary group-hover:underline">
                                        {noneDone ? "Start" : allDone ? "Review" : "Continue"}
                                    </span>
                                </div>
                            </button>
                        );
                    })}
                </div>
            )}
        </div>
    );
}

// ── Level 2: Activity List Page ───────────────────────────────────────────────

function ActivityListPage({
    courseId,
    module,
    onBack,
    onOpenActivity,
}: {
    courseId: string;
    module: Module;
    onBack: () => void;
    onOpenActivity: (activityId: string) => void;
}) {
    const modActivities = courseActivities
        .filter((a) => a.moduleId === module.id)
        .sort((a, b) => a.order - b.order);
    const { completed, total, pct } = moduleCompletion(module.id);

    return (
        <div className="space-y-6">
            {/* Module header */}
            <div className="space-y-3">
                <div className="flex items-center gap-2 text-sm">
                    <span className="text-xs font-medium text-muted-foreground">Module {module.order}</span>
                </div>
                <h1 className="text-2xl font-bold tracking-tight">{module.title}</h1>
                <div className="flex items-center gap-4 flex-wrap">
                    <div className="flex items-center gap-3">
                        <div className="text-sm">
                            <span className="font-bold text-lg">{pct}%</span>
                            <span className="text-muted-foreground ml-2">{completed} of {total} activities</span>
                        </div>
                        <Progress value={pct} className="w-32 h-2" />
                    </div>
                </div>
            </div>

            <Separator />

            {/* Activity rows */}
            <div className="space-y-2">
                {modActivities.map((activity, i) => {
                    const done = isActivityCompleted(activity.id);
                    const meta = getActivityMeta(activity);
                    const dueDate = meta?.dueDate;
                    const isPastDue = dueDate && new Date(dueDate) < new Date() && !done;

                    return (
                        <button
                            key={activity.id}
                            onClick={() => onOpenActivity(activity.id)}
                            className="group w-full text-left flex items-center gap-4 rounded-lg border bg-card p-4 transition-all hover:border-primary/40 hover:shadow-sm focus:outline-none focus:ring-2 focus:ring-ring"
                        >
                            {/* Index / completion */}
                            <div className="shrink-0 flex items-center justify-center">
                                {done ? (
                                    <CheckCircle2Icon className="size-6 text-primary" />
                                ) : (
                                    <div className="size-6 rounded-full border-2 border-muted-foreground/30 flex items-center justify-center text-xs font-semibold text-muted-foreground">
                                        {i + 1}
                                    </div>
                                )}
                            </div>

                            {/* Type icon */}
                            <div className={cn(
                                "size-10 rounded-lg flex items-center justify-center shrink-0",
                                done ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"
                            )}>
                                {activityIcon(activity.type, "size-5")}
                            </div>

                            {/* Title + meta */}
                            <div className="flex-1 min-w-0 space-y-1">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <h3 className={cn("font-medium leading-tight", done && "text-muted-foreground line-through")}>
                                        {activity.title}
                                    </h3>
                                    <Badge variant={activityTypeBadgeVariant(activity.type)} className="text-xs">
                                        {activityTypeLabel(activity.type)}
                                    </Badge>
                                </div>
                                <div className="flex items-center gap-3 text-xs text-muted-foreground flex-wrap">
                                    {meta?.duration && (
                                        <span className="flex items-center gap-1">
                                            <ClockIcon className="size-3" />
                                            {meta.duration}
                                        </span>
                                    )}
                                    {dueDate && !done && (
                                        <span className={cn("flex items-center gap-1", isPastDue && "text-destructive font-medium")}>
                                            {isPastDue ? <AlertCircleIcon className="size-3" /> : <CalendarIcon className="size-3" />}
                                            {isPastDue ? "Overdue" : `Due ${dueDate}`}
                                        </span>
                                    )}
                                    {meta?.maxScore && activity.type === "assignment" && (
                                        <span className="flex items-center gap-1">
                                            <GraduationCapIcon className="size-3" />
                                            {meta.maxScore} pts
                                        </span>
                                    )}
                                    {meta?.maxScore && activity.type === "assessment" && (
                                        <span className="flex items-center gap-1">
                                            <GraduationCapIcon className="size-3" />
                                            Pass {meta.maxScore}%
                                        </span>
                                    )}
                                    {meta?.difficulty && (
                                        <Badge className={cn("text-xs", meta.difficulty === "easy" && "bg-success/10 text-success border-success/20", meta.difficulty === "medium" && "bg-warning/10 text-warning-foreground border-warning/20", meta.difficulty === "hard" && "bg-destructive/10 text-destructive border-destructive/20")} variant="outline">
                                            {meta.difficulty}
                                        </Badge>
                                    )}
                                    {done && (
                                        <span className="flex items-center gap-1 text-success">
                                            <CheckCircle2Icon className="size-3" />
                                            Completed
                                        </span>
                                    )}
                                </div>
                            </div>

                            <ChevronRightIcon className="size-5 text-muted-foreground shrink-0 group-hover:text-foreground group-hover:translate-x-0.5 transition-all" />
                        </button>
                    );
                })}
            </div>
        </div>
    );
}

// ── Level 3: Activity Content Page ────────────────────────────────────────────

function ActivityContentPage({
    activity,
    courseId,
    module,
    allFlatActivities,
    onBack,
    onNavigate,
}: {
    activity: CourseActivity;
    courseId: string;
    module: Module;
    allFlatActivities: CourseActivity[];
    onBack: () => void;
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
                        {meta.weight != null && (
                            <span className="flex items-center gap-1.5">
                                <BarChart3Icon className="size-4" />
                                {meta.weight}% of grade
                            </span>
                        )}
                        {meta.difficulty && (
                            <Badge className={cn("text-xs", meta.difficulty === "easy" && "bg-success/10 text-success border-success/20", meta.difficulty === "medium" && "bg-warning/10 text-warning-foreground border-warning/20", meta.difficulty === "hard" && "bg-destructive/10 text-destructive border-destructive/20")} variant="outline">
                                {meta.difficulty}
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
            <div className="flex items-center justify-between pt-2 pb-6 gap-2">
                <Button
                    variant="outline"
                    disabled={!prev}
                    onClick={() => prev && onNavigate(prev.id)}
                    className="gap-2 min-w-0"
                >
                    <ArrowLeftIcon className="size-4 shrink-0" />
                    <span className="truncate">{prev ? prev.title : "Previous"}</span>
                </Button>

                {!completed && (
                    <Button variant="default" className="gap-2 shrink-0">
                        <CheckCircle2Icon className="size-4" />
                        <span className="hidden sm:inline">Mark as Complete</span>
                        <span className="sm:hidden">Complete</span>
                    </Button>
                )}

                <Button
                    variant={next ? "default" : "outline"}
                    disabled={!next}
                    onClick={() => next && onNavigate(next.id)}
                    className="gap-2 min-w-0"
                >
                    <span className="truncate">{next ? next.title : "Next"}</span>
                    <ArrowRightIcon className="size-4 shrink-0" />
                </Button>
            </div>
        </div>
    );
}

// ── Root component ────────────────────────────────────────────────────────────

type LearnView = "modules" | "activities" | "content";

function MockCourseWorkspace() {
    const { courseId } = useParams<{ courseId: string }>();
    const course = courses.find((c) => c.id === courseId) ?? courses[0];
    const instructor = instructors.find((i) => i.id === course.instructorId);
    const courseAnnouncements = announcements.filter((a) => a.courseId === course.id);

    const courseModules = modules.filter((m) => m.courseId === course.id).sort((a, b) => a.order - b.order);
    const allFlatActivities = courseModules.flatMap((mod) =>
        courseActivities.filter((a) => a.moduleId === mod.id).sort((a, b) => a.order - b.order)
    );

    const [learnView, setLearnView] = React.useState<LearnView>("modules");
    const [selectedModuleId, setSelectedModuleId] = React.useState<string | null>(null);
    const [selectedActivityId, setSelectedActivityId] = React.useState<string | null>(null);
    const [topTab, setTopTab] = React.useState("learn");

    const selectedModule = courseModules.find((m) => m.id === selectedModuleId) ?? null;
    const selectedActivity = allFlatActivities.find((a) => a.id === selectedActivityId) ?? null;

    const { completed: cc, total: ct, pct: cp } = courseCompletion(course.id);

    // Flat activities within the selected module (for prev/next within module)
    const moduleFlatActivities = selectedModule
        ? courseActivities.filter((a) => a.moduleId === selectedModule.id).sort((a, b) => a.order - b.order)
        : [];

    const openModule = (moduleId: string) => {
        setSelectedModuleId(moduleId);
        setSelectedActivityId(null);
        setLearnView("activities");
    };

    const openActivity = (activityId: string) => {
        setSelectedActivityId(activityId);
        setLearnView("content");
    };

    const backToModules = () => {
        setSelectedModuleId(null);
        setSelectedActivityId(null);
        setLearnView("modules");
    };

    const backToActivities = () => {
        setSelectedActivityId(null);
        setLearnView("activities");
    };

    // Build breadcrumb items based on current learn view
    const breadcrumbItems: { label: string; onClick?: () => void }[] = [
        { label: "Modules", onClick: topTab === "learn" ? backToModules : undefined },
    ];
    if (selectedModule) {
        breadcrumbItems.push({ label: selectedModule.title, onClick: learnView === "content" ? backToActivities : undefined });
    }
    if (selectedActivity) {
        breadcrumbItems.push({ label: selectedActivity.title });
    }

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
                            <p className="text-sm font-bold">{cp}%</p>
                        </div>
                        <Progress value={cp} className="w-24 h-2" />
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
            <ScrollArea className="flex-1">
                <div className="max-w-4xl mx-auto px-6 lg:px-10 py-6">
                    {/* Breadcrumb (only for Learn tab) */}
                    {topTab === "learn" && learnView !== "modules" && (
                        <div className="mb-4">
                            <Breadcrumb items={breadcrumbItems} />
                        </div>
                    )}

                    {topTab === "learn" && learnView === "modules" && (
                        <ModuleListPage courseId={course.id} onOpenModule={openModule} />
                    )}

                    {topTab === "learn" && learnView === "activities" && selectedModule && (
                        <ActivityListPage
                            courseId={course.id}
                            module={selectedModule}
                            onBack={backToModules}
                            onOpenActivity={openActivity}
                        />
                    )}

                    {topTab === "learn" && learnView === "content" && selectedActivity && selectedModule && (
                        <ActivityContentPage
                            activity={selectedActivity}
                            courseId={course.id}
                            module={selectedModule}
                            allFlatActivities={moduleFlatActivities}
                            onBack={backToActivities}
                            onNavigate={openActivity}
                        />
                    )}

                    {topTab === "announcements" && <AnnouncementsTab courseId={course.id} />}
                    {topTab === "people" && <PeopleTab courseId={course.id} />}
                    {topTab === "grades" && <GradesTab courseId={course.id} />}
                    {topTab === "progress" && <ProgressTab courseId={course.id} />}
                </div>
            </ScrollArea>
        </div>
    );
}

const WORKSPACE_PAGE_SIZE = 10;

function resourceTypeLabel(type: ApiActivityType): string {
    return ["Lesson", "Assignment", "Assessment", "Coding Problem"][type] ?? "Activity";
}

function resourceIcon(type: ApiActivityType, className?: string) {
    const cls = cn("size-4 shrink-0", className);
    switch (type) {
        case ApiActivityType.Lesson: return <PlayCircleIcon className={cls} />;
        case ApiActivityType.Assignment: return <FileTextIcon className={cls} />;
        case ApiActivityType.Assessment: return <ClipboardCheckIcon className={cls} />;
        case ApiActivityType.Problem: return <CodeIcon className={cls} />;
        default: return <BookOpenIcon className={cls} />;
    }
}

async function workspaceRead<T>(request: Promise<T>): Promise<T> {
    try {
        return await request;
    } catch (error) {
        if (error instanceof ApiError && error.status === 404) {
            throw new Error("This course or module is unavailable. It may no longer be published or accessible to your account.");
        }
        throw error;
    }
}

function WorkspacePagination({ page, total, onChange }: { page: number; total: number; onChange: (page: number) => void }) {
    const totalPages = Math.max(1, Math.ceil(total / WORKSPACE_PAGE_SIZE));
    if (totalPages === 1) return null;
    return (
        <nav aria-label="Content pages" className="flex items-center justify-center gap-3 pt-3">
            <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => onChange(page - 1)}>
                <ChevronLeftIcon className="size-4" /> Previous
            </Button>
            <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
            <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => onChange(page + 1)}>
                Next <ChevronRightIcon className="size-4" />
            </Button>
        </nav>
    );
}

function WorkspaceModules({ courseId, page, initialPage, onPageChange, onOpen }: {
    courseId: string;
    page: number;
    initialPage: PaginatedResponse<ModuleResponse> | null;
    onPageChange: (page: number) => void;
    onOpen: (module: ModuleResponse) => void;
}) {
    const { data, loading, error, reload } = useApi(() => initialPage
        ? Promise.resolve(initialPage)
        : workspaceRead(courseApi.getCourseModules(courseId, page, WORKSPACE_PAGE_SIZE)));
    if (loading) return <LoadingState label="Loading modules..." />;
    if (error) return <ErrorState message={error} onRetry={reload} />;
    if (!data) return null;

    return (
        <div className="space-y-2">
            {data.data.length === 0 && (
                <Card><CardContent className="py-8 text-sm text-muted-foreground">No published modules are available on this page.</CardContent></Card>
            )}
            {data.data.map((module, index) => (
                <button key={module.id} className="block w-full text-left rounded-lg focus:outline-none focus:ring-2 focus:ring-ring" onClick={() => onOpen(module)}>
                    <Card className="transition-all hover:border-primary/40 hover:shadow-sm">
                        <CardContent className="flex items-center gap-3 py-4">
                            <div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
                                {(page - 1) * WORKSPACE_PAGE_SIZE + index + 1}
                            </div>
                            <div>
                                <h3 className="font-medium">{module.title}</h3>
                                {module.description && <p className="text-sm text-muted-foreground">{module.description}</p>}
                            </div>
                            <ChevronRightIcon className="ml-auto size-5 text-muted-foreground" />
                        </CardContent>
                    </Card>
                </button>
            ))}
            <WorkspacePagination page={page} total={data.maxCount} onChange={onPageChange} />
        </div>
    );
}

function WorkspaceActivities({ module, page, onPageChange, onBack }: {
    module: ModuleResponse;
    page: number;
    onPageChange: (page: number) => void;
    onBack: () => void;
}) {
    const { data, loading, error, reload } = useApi(() =>
        workspaceRead(courseApi.getModuleActivities(module.id, page, WORKSPACE_PAGE_SIZE)));
    const [selectedActivity, setSelectedActivity] = React.useState<ActivityResponse | null>(null);

    return (
        <div className="space-y-3">
            <Breadcrumb items={[
                { label: "Modules", onClick: onBack },
                { label: module.title, onClick: selectedActivity ? () => setSelectedActivity(null) : undefined },
                ...(selectedActivity ? [{ label: selectedActivity.title }] : []),
            ]} />
            {loading ? <LoadingState label="Loading module activities..." /> : error ? (
                <ErrorState message={error} onRetry={reload} />
            ) : selectedActivity ? (
                <div className="space-y-6">
                    <div className="flex items-center gap-2">
                        <Badge variant="secondary" className="gap-1.5">
                            {resourceIcon(selectedActivity.type, "size-3")}
                            {resourceTypeLabel(selectedActivity.type)}
                        </Badge>
                        <h2 className="text-2xl font-bold tracking-tight">{selectedActivity.title}</h2>
                    </div>
                    <Card><CardContent className="py-8 text-sm text-muted-foreground">
                        You can browse this activity's title and type here. Its content and participation screen are not included in this workspace yet.
                    </CardContent></Card>
                    <Button variant="outline" onClick={() => setSelectedActivity(null)}>
                        <ArrowLeftIcon className="size-4" /> Back to activities
                    </Button>
                </div>
            ) : data ? (
                <div className="space-y-2">
                    {module.description && <p className="text-sm text-muted-foreground">{module.description}</p>}
                    {data.data.length === 0 && (
                        <Card><CardContent className="py-8 text-sm text-muted-foreground">No published activities are available on this page.</CardContent></Card>
                    )}
                    {data.data.map((activity, index) => (
                        <button key={activity.id} onClick={() => setSelectedActivity(activity)} className="group w-full text-left flex items-center gap-4 rounded-lg border bg-card p-4 transition-all hover:border-primary/40 hover:shadow-sm focus:outline-none focus:ring-2 focus:ring-ring">
                            <div className="size-10 rounded-lg flex items-center justify-center shrink-0 bg-muted text-muted-foreground">
                                {resourceIcon(activity.type, "size-5")}
                            </div>
                            <div className="flex-1 min-w-0 space-y-1">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <h3 className="font-medium leading-tight">{activity.title}</h3>
                                    <Badge variant="secondary" className="text-xs">{resourceTypeLabel(activity.type)}</Badge>
                                </div>
                                <p className="text-xs text-muted-foreground">Activity {(page - 1) * WORKSPACE_PAGE_SIZE + index + 1}</p>
                            </div>
                            <ChevronRightIcon className="size-5 text-muted-foreground shrink-0 group-hover:text-foreground" />
                        </button>
                    ))}
                    <WorkspacePagination page={page} total={data.maxCount} onChange={onPageChange} />
                </div>
            ) : null}
        </div>
    );
}

function RealCourseWorkspace({ courseId }: { courseId: string }) {
    const { data, loading, error, reload } = useApi(async () => {
        const [course, modules] = await workspaceRead(Promise.all([
            courseApi.getCourseMetadata(courseId),
            courseApi.getCourseModules(courseId, 1, WORKSPACE_PAGE_SIZE),
        ]));
        return { course, modules };
    });
    const [modulePage, setModulePage] = React.useState(1);
    const [activityPage, setActivityPage] = React.useState(1);
    const [selectedModule, setSelectedModule] = React.useState<ModuleResponse | null>(null);
    // The initial page is used once; later visits fetch again to recheck visibility.
    const [useInitialPage, setUseInitialPage] = React.useState(true);

    const openModule = (module: ModuleResponse) => {
        setUseInitialPage(false);
        setActivityPage(1);
        setSelectedModule(module);
    };
    const backToModules = () => {
        setSelectedModule(null);
        setActivityPage(1);
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center gap-3">
                <Link to="/student/courses" aria-label="Back to My Courses" className="text-muted-foreground hover:text-foreground">
                    <ArrowLeftIcon className="size-4" />
                </Link>
                {data && !loading && !error && (
                    <div>
                        <Badge variant="success">Published</Badge>
                        <h1 className="mt-2 text-2xl font-bold tracking-tight">{data.course.title}</h1>
                        <p className="text-sm text-muted-foreground">{`${data.course.instructorFirstName} ${data.course.instructorLastName}`.trim()}</p>
                    </div>
                )}
            </div>
            {loading ? <LoadingState label="Loading course..." /> : error ? (
                <ErrorState message={error} onRetry={reload} />
            ) : data ? (
                <>
                    {data.course.description && (
                        <Card>
                            <CardHeader><CardTitle>About this course</CardTitle></CardHeader>
                            <CardContent><p className="text-sm text-muted-foreground">{data.course.description}</p></CardContent>
                        </Card>
                    )}
                    {selectedModule ? (
                        <WorkspaceActivities key={`${selectedModule.id}:${activityPage}`} module={selectedModule} page={activityPage} onPageChange={setActivityPage} onBack={backToModules} />
                    ) : (
                        <div className="space-y-3">
                            <h2 className="text-lg font-semibold">Modules</h2>
                            <WorkspaceModules key={modulePage} courseId={courseId} page={modulePage}
                                initialPage={useInitialPage && modulePage === 1 ? data.modules : null}
                                onPageChange={(page) => { setUseInitialPage(false); setModulePage(page); }} onOpen={openModule} />
                        </div>
                    )}
                </>
            ) : null}
        </div>
    );
}

export function CourseWorkspace() {
    const { courseId } = useParams<{ courseId: string }>();
    // Remount all selection, pagination, and request state before a new course renders.
    return <RealCourseWorkspace key={courseId} courseId={courseId ?? ""} />;
}
