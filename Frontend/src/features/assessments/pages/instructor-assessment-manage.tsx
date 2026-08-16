import * as React from "react";
import { useParams, Link } from "react-router-dom";
import {
  ArrowLeftIcon,
  PlusIcon,
  Trash2Icon,
  GripVerticalIcon,
  ClockIcon,
  TrophyIcon,
  UsersIcon,
  CheckCircleIcon,
  EyeIcon,
  SaveIcon,
  AlertCircleIcon,
  ClipboardListIcon,
  SettingsIcon,
  FileTextIcon,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { PageHeader } from "@/shared/components/page-header";
import { assessments, courses } from "@/shared/data/courses";
import {
  getQuestionsByAssessment,
  getAssessmentLinks,
  getAttemptsByAssessment,
  questionTypeLabels,
  getQuestionById,
} from "@/shared/data/question-bank";
import { students } from "@/shared/data/users";
import { QuestionPickerDialog } from "../components/question-picker-dialog";
import { QuestionAnswerRenderer } from "../components/question-answer-renderer";
import { questionTypeIconMap } from "../components/question-answer-renderer";
import { cn } from "@/lib/utils";
import type { Assessment, Question } from "@/types";

export function InstructorAssessmentManage() {
  const { assessmentId } = useParams<{ assessmentId: string }>();
  const assessment = assessments.find((a) => a.id === assessmentId);
  const course = assessment ? courses.find((c) => c.id === "cs-101") : null;

  const [pickerOpen, setPickerOpen] = React.useState(false);
  const [draft, setDraft] = React.useState<Assessment | null>(assessment ?? null);
  const [saved, setSaved] = React.useState(false);

  const linkedQuestions = assessment ? getQuestionsByAssessment(assessment.id) : [];
  const links = assessment ? getAssessmentLinks(assessment.id) : [];
  const attempts = assessment ? getAttemptsByAssessment(assessment.id) : [];

  if (!assessment || !draft) {
    return (
      <div className="flex items-center justify-center h-[60vh]">
        <div className="text-center space-y-3">
          <AlertCircleIcon className="size-10 text-muted-foreground mx-auto" />
          <p className="text-muted-foreground">Assessment not found.</p>
          <Button asChild variant="outline">
            <Link to="/instructor/assessments">Back to Assessments</Link>
          </Button>
        </div>
      </div>
    );
  }

  const totalPoints = links.reduce((sum, l) => sum + l.points, 0);
  const submittedAttempts = attempts.filter((a) => a.status === "submitted" || a.status === "graded");
  const needsGrading = attempts.filter((a) => a.status === "submitted");
  const gradedAttempts = attempts.filter((a) => a.status === "graded");
  const avgScore = gradedAttempts.length > 0
    ? gradedAttempts.reduce((s, a) => s + ((a.score ?? 0) / a.maxScore) * 100, 0) / gradedAttempts.length
    : 0;

  const handleAddQuestions = (questionIds: string[]) => {
    // In a real app, this would call the backend. Here we just close.
    setPickerOpen(false);
  };

  const handleSave = () => {
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  const updateDraft = (field: keyof Assessment, value: string | number | boolean) => {
    setDraft((prev) => prev ? { ...prev, [field]: value } : prev);
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title={assessment.title}
        description={assessment.description}
        breadcrumbs={[
          { label: "Dashboard", href: "/instructor" },
          { label: "Assessments", href: "/instructor/assessments" },
          { label: assessment.title },
        ]}
        actions={
          <Button variant="outline" asChild>
            <Link to="/instructor/assessments">
              <ArrowLeftIcon className="size-4 mr-2" />Back
            </Link>
          </Button>
        }
      />

      {/* Summary stat cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <ClipboardListIcon className="size-4" />Questions
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{linkedQuestions.length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <TrophyIcon className="size-4" />Total Points
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{totalPoints}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <UsersIcon className="size-4" />Attempts
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{submittedAttempts.length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <CheckCircleIcon className="size-4 text-warning" />Needs Grading
            </CardTitle>
          </CardHeader>
          <CardContent><p className="text-2xl font-bold">{needsGrading.length}</p></CardContent>
        </Card>
      </div>

      <Tabs defaultValue="overview">
        <TabsList>
          <TabsTrigger value="overview"><SettingsIcon className="size-3.5 mr-1" />Overview</TabsTrigger>
          <TabsTrigger value="questions"><ClipboardListIcon className="size-3.5 mr-1" />Questions</TabsTrigger>
          <TabsTrigger value="settings"><SettingsIcon className="size-3.5 mr-1" />Settings</TabsTrigger>
          <TabsTrigger value="attempts"><FileTextIcon className="size-3.5 mr-1" />Attempts{needsGrading.length > 0 && ` (${needsGrading.length})`}</TabsTrigger>
        </TabsList>

        {/* Overview Tab */}
        <TabsContent value="overview" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Assessment Details</CardTitle>
              <CardDescription>Summary of this assessment's configuration</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Status</Label>
                  <Badge variant={assessment.status === "published" ? "success" : "secondary"}>
                    {assessment.status ?? "draft"}
                  </Badge>
                </div>
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Duration</Label>
                  <p className="text-sm font-medium">{assessment.duration} min</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Passing Score</Label>
                  <p className="text-sm font-medium">{assessment.passingScore}%</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Max Attempts</Label>
                  <p className="text-sm font-medium">{assessment.maxAttempts}</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Due Date</Label>
                  <p className="text-sm font-medium">{assessment.dueDate}</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Available From</Label>
                  <p className="text-sm font-medium">{assessment.availableFrom ?? "—"}</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Available To</Label>
                  <p className="text-sm font-medium">{assessment.availableTo ?? "—"}</p>
                </div>
                <div className="space-y-1">
                  <Label className="text-xs text-muted-foreground">Result Visibility</Label>
                  <p className="text-sm font-medium capitalize">{assessment.resultVisibility?.replace("-", " ") ?? "immediate"}</p>
                </div>
              </div>
              <Separator />
              <div className="grid grid-cols-2 gap-4">
                <div className="flex items-center justify-between rounded-lg border p-3">
                  <div>
                    <p className="text-sm font-medium">Shuffle Questions</p>
                    <p className="text-xs text-muted-foreground">Randomize question order</p>
                  </div>
                  <Switch checked={assessment.shuffleQuestions ?? false} disabled />
                </div>
                <div className="flex items-center justify-between rounded-lg border p-3">
                  <div>
                    <p className="text-sm font-medium">Shuffle Answers</p>
                    <p className="text-xs text-muted-foreground">Randomize answer options</p>
                  </div>
                  <Switch checked={assessment.shuffleAnswers ?? false} disabled />
                </div>
              </div>
              {course && (
                <>
                  <Separator />
                  <div className="flex items-center gap-2 text-sm">
                    <span className="text-muted-foreground">Course:</span>
                    <Badge variant="outline">{course.code}</Badge>
                    <span>{course.title}</span>
                  </div>
                </>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Performance Summary</CardTitle>
            </CardHeader>
            <CardContent>
              {gradedAttempts.length === 0 ? (
                <p className="text-sm text-muted-foreground">No graded attempts yet.</p>
              ) : (
                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <p className="text-xs text-muted-foreground">Average Score</p>
                    <p className="text-xl font-bold">{avgScore.toFixed(1)}%</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">Highest</p>
                    <p className="text-xl font-bold">
                      {Math.max(...gradedAttempts.map((a) => ((a.score ?? 0) / a.maxScore) * 100)).toFixed(1)}%
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground">Lowest</p>
                    <p className="text-xl font-bold">
                      {Math.min(...gradedAttempts.map((a) => ((a.score ?? 0) / a.maxScore) * 100)).toFixed(1)}%
                    </p>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Questions Tab */}
        <TabsContent value="questions" className="space-y-4">
          <Card>
            <CardHeader className="flex-row items-center justify-between">
              <div>
                <CardTitle>Questions ({linkedQuestions.length})</CardTitle>
                <CardDescription>Add, remove, and reorder questions</CardDescription>
              </div>
              <Button onClick={() => setPickerOpen(true)}>
                <PlusIcon className="size-4 mr-1" />Add from Bank
              </Button>
            </CardHeader>
            <CardContent>
              {linkedQuestions.length === 0 ? (
                <div className="py-12 text-center space-y-3">
                  <ClipboardListIcon className="size-10 text-muted-foreground mx-auto" />
                  <p className="text-sm text-muted-foreground">No questions yet. Add some from the question bank.</p>
                  <Button onClick={() => setPickerOpen(true)}>
                    <PlusIcon className="size-4 mr-1" />Add Questions
                  </Button>
                </div>
              ) : (
                <div className="space-y-2">
                  {linkedQuestions.map((q, i) => {
                    const Icon = questionTypeIconMap[q.type];
                    const link = links.find((l) => l.questionId === q.id);
                    return (
                      <div key={q.id} className="flex items-start gap-3 rounded-lg border p-3">
                        <div className="flex items-center gap-2 pt-1">
                          <GripVerticalIcon className="size-4 text-muted-foreground cursor-grab" />
                          <span className="text-xs font-medium text-muted-foreground w-6">{i + 1}</span>
                        </div>
                        <div className="flex-1 min-w-0 space-y-1">
                          <div className="flex items-center gap-2 flex-wrap">
                            <Icon className="size-3.5 text-muted-foreground" />
                            <span className="text-sm font-medium truncate">{q.title}</span>
                            <Badge variant="outline" className="text-xs">{link?.points ?? q.points} pts</Badge>
                            <Badge
                              variant="outline"
                              className={cn(
                                "text-xs",
                                q.difficulty === "easy" && "border-success/20 text-success",
                                q.difficulty === "medium" && "border-warning/20 text-warning-foreground",
                                q.difficulty === "hard" && "border-destructive/20 text-destructive"
                              )}
                            >
                              {q.difficulty}
                            </Badge>
                            <Badge variant="secondary" className="text-xs">{questionTypeLabels[q.type]}</Badge>
                          </div>
                          <p className="text-xs text-muted-foreground line-clamp-1">{q.text}</p>
                        </div>
                        <Button variant="ghost" size="icon" className="size-8 shrink-0">
                          <Trash2Icon className="size-3.5 text-destructive" />
                        </Button>
                      </div>
                    );
                  })}
                </div>
              )}
            </CardContent>
          </Card>

          <QuestionPickerDialog
            open={pickerOpen}
            onOpenChange={setPickerOpen}
            onAdd={handleAddQuestions}
            excludeIds={linkedQuestions.map((q) => q.id)}
          />
        </TabsContent>

        {/* Settings Tab */}
        <TabsContent value="settings" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Assessment Settings</CardTitle>
              <CardDescription>Edit the configuration of this assessment</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="space-y-2">
                <Label htmlFor="title">Title</Label>
                <Input id="title" value={draft.title} onChange={(e) => updateDraft("title", e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description</Label>
                <Textarea id="description" value={draft.description ?? ""} onChange={(e) => updateDraft("description", e.target.value)} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="duration">Duration (minutes)</Label>
                  <Input id="duration" type="number" value={draft.duration} onChange={(e) => updateDraft("duration", Number(e.target.value))} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="passingScore">Passing Score (%)</Label>
                  <Input id="passingScore" type="number" value={draft.passingScore} onChange={(e) => updateDraft("passingScore", Number(e.target.value))} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="maxAttempts">Max Attempts</Label>
                  <Input id="maxAttempts" type="number" value={draft.maxAttempts} onChange={(e) => updateDraft("maxAttempts", Number(e.target.value))} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="dueDate">Due Date</Label>
                  <Input id="dueDate" type="date" value={draft.dueDate} onChange={(e) => updateDraft("dueDate", e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="availableFrom">Available From</Label>
                  <Input id="availableFrom" type="date" value={draft.availableFrom ?? ""} onChange={(e) => updateDraft("availableFrom", e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="availableTo">Available To</Label>
                  <Input id="availableTo" type="date" value={draft.availableTo ?? ""} onChange={(e) => updateDraft("availableTo", e.target.value)} />
                </div>
              </div>
              <Separator />
              <div className="space-y-4">
                <div className="flex items-center justify-between rounded-lg border p-3">
                  <div>
                    <Label className="text-sm font-medium">Shuffle Questions</Label>
                    <p className="text-xs text-muted-foreground">Randomize question order for each student</p>
                  </div>
                  <Switch
                    checked={draft.shuffleQuestions ?? false}
                    onCheckedChange={(v) => updateDraft("shuffleQuestions", v)}
                  />
                </div>
                <div className="flex items-center justify-between rounded-lg border p-3">
                  <div>
                    <Label className="text-sm font-medium">Shuffle Answers</Label>
                    <p className="text-xs text-muted-foreground">Randomize answer option order</p>
                  </div>
                  <Switch
                    checked={draft.shuffleAnswers ?? false}
                    onCheckedChange={(v) => updateDraft("shuffleAnswers", v)}
                  />
                </div>
                <div className="space-y-2">
                  <Label>Result Visibility</Label>
                  <Select
                    value={draft.resultVisibility ?? "immediate"}
                    onValueChange={(v) => updateDraft("resultVisibility", v)}
                  >
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      <SelectItem value="immediate">Immediate — students see results on submit</SelectItem>
                      <SelectItem value="after-deadline">After Deadline — visible after due date</SelectItem>
                      <SelectItem value="manual">Manual — instructor releases results</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="flex justify-end gap-2">
                <Button variant="outline">Cancel</Button>
                <Button onClick={handleSave}>
                  <SaveIcon className="size-4 mr-1" />
                  {saved ? "Saved!" : "Save Changes"}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Attempts Tab */}
        <TabsContent value="attempts" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Student Attempts ({submittedAttempts.length})</CardTitle>
              <CardDescription>Review and grade student submissions</CardDescription>
            </CardHeader>
            <CardContent>
              {submittedAttempts.length === 0 ? (
                <p className="text-sm text-muted-foreground py-8 text-center">No attempts submitted yet.</p>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Student</TableHead>
                      <TableHead>Attempt</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Score</TableHead>
                      <TableHead>Time Spent</TableHead>
                      <TableHead>Submitted</TableHead>
                      <TableHead></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {submittedAttempts.map((attempt) => {
                      const student = students.find((s) => s.id === attempt.studentId);
                      const scorePct = attempt.score !== null
                        ? Math.round((attempt.score / attempt.maxScore) * 100)
                        : null;
                      return (
                        <TableRow key={attempt.id}>
                          <TableCell className="font-medium">
                            {student ? `${student.firstName} ${student.lastName}` : attempt.studentId}
                          </TableCell>
                          <TableCell className="text-muted-foreground">#{attempt.attemptNumber}</TableCell>
                          <TableCell>
                            <Badge
                              variant={attempt.status === "graded" ? "success" : "warning"}
                            >
                              {attempt.status}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            {scorePct !== null ? (
                              <span className={cn("font-medium", scorePct >= assessment.passingScore ? "text-success" : "text-destructive")}>
                                {attempt.score}/{attempt.maxScore} ({scorePct}%)
                              </span>
                            ) : (
                              <span className="text-muted-foreground">Pending</span>
                            )}
                          </TableCell>
                          <TableCell className="text-muted-foreground">
                            <span className="flex items-center gap-1">
                              <ClockIcon className="size-3" />{attempt.timeSpent ?? "—"} min
                            </span>
                          </TableCell>
                          <TableCell className="text-muted-foreground text-xs">
                            {attempt.submittedAt ? new Date(attempt.submittedAt).toLocaleString() : "—"}
                          </TableCell>
                          <TableCell>
                            <Button variant="ghost" size="sm" asChild>
                              <Link to={`/instructor/assessments/${assessment.id}/attempts/${attempt.id}`}>
                                <EyeIcon className="size-3.5 mr-1" />Review
                              </Link>
                            </Button>
                          </TableCell>
                        </TableRow>
                      );
                    })}
                  </TableBody>
                </Table>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
