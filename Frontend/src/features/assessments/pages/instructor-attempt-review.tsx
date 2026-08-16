import * as React from "react";
import { useParams, Link } from "react-router-dom";
import {
  ArrowLeftIcon,
  ClockIcon,
  CheckCircleIcon,
  XCircleIcon,
  SaveIcon,
  AlertCircleIcon,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import { assessments } from "@/shared/data/courses";
import {
  assessmentAttempts,
  getQuestionsByAssessment,
  getQuestionById,
  questionTypeLabels,
} from "@/shared/data/question-bank";
import { students } from "@/shared/data/users";
import { QuestionAnswerRenderer } from "../components/question-answer-renderer";
import { cn } from "@/lib/utils";
import type { AttemptAnswer } from "@/types";

export function InstructorAttemptReview() {
  const { assessmentId, attemptId } = useParams<{ assessmentId: string; attemptId: string }>();
  const assessment = assessments.find((a) => a.id === assessmentId);
  const attempt = assessmentAttempts.find((a) => a.id === attemptId);
  const student = attempt ? students.find((s) => s.id === attempt.studentId) : null;
  const questions = assessment ? getQuestionsByAssessment(assessment.id) : [];

  const [gradedAnswers, setGradedAnswers] = React.useState<Record<string, { score: string; feedback: string }>>({});
  const [saved, setSaved] = React.useState(false);

  React.useEffect(() => {
    if (attempt) {
      const initial: Record<string, { score: string; feedback: string }> = {};
      attempt.answers.forEach((a) => {
        initial[a.questionId] = {
          score: a.score?.toString() ?? "",
          feedback: a.feedback ?? "",
        };
      });
      setGradedAnswers(initial);
    }
  }, [attempt]);

  if (!assessment || !attempt || !student) {
    return (
      <div className="flex items-center justify-center h-[60vh]">
        <div className="text-center space-y-3">
          <AlertCircleIcon className="size-10 text-muted-foreground mx-auto" />
          <p className="text-muted-foreground">Attempt not found.</p>
          <Button asChild variant="outline">
            <Link to={`/instructor/assessments/${assessmentId}`}>Back to Assessment</Link>
          </Button>
        </div>
      </div>
    );
  }

  const scorePct = attempt.score !== null
    ? Math.round((attempt.score / attempt.maxScore) * 100)
    : null;
  const passed = scorePct !== null && scorePct >= assessment.passingScore;

  const updateScore = (questionId: string, score: string) => {
    setGradedAnswers((prev) => ({
      ...prev,
      [questionId]: { ...prev[questionId], score },
    }));
  };

  const updateFeedback = (questionId: string, feedback: string) => {
    setGradedAnswers((prev) => ({
      ...prev,
      [questionId]: { ...prev[questionId], feedback },
    }));
  };

  const handleSaveGrades = () => {
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  const needsManualGrading = questions.filter((q) => q.type === "essay" || q.type === "short-answer");
  const autoGraded = questions.filter((q) => q.type !== "essay" && q.type !== "short-answer");

  return (
    <div className="space-y-6">
      <PageHeader
        title="Attempt Review"
        description={`${student.firstName} ${student.lastName} — Attempt #${attempt.attemptNumber}`}
        breadcrumbs={[
          { label: "Dashboard", href: "/instructor" },
          { label: "Assessments", href: "/instructor/assessments" },
          { label: assessment.title, href: `/instructor/assessments/${assessment.id}` },
          { label: "Review" },
        ]}
        actions={
          <Button variant="outline" asChild>
            <Link to={`/instructor/assessments/${assessment.id}`}>
              <ArrowLeftIcon className="size-4 mr-2" />Back
            </Link>
          </Button>
        }
      />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Status</CardTitle></CardHeader>
          <CardContent>
            <Badge variant={attempt.status === "graded" ? "success" : "warning"}>{attempt.status}</Badge>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Score</CardTitle></CardHeader>
          <CardContent>
            {scorePct !== null ? (
              <p className={cn("text-2xl font-bold", passed ? "text-success" : "text-destructive")}>
                {attempt.score}/{attempt.maxScore}
              </p>
            ) : (
              <p className="text-2xl font-bold text-muted-foreground">Pending</p>
            )}
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2">
            <ClockIcon className="size-4" />Time Spent</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{attempt.timeSpent ?? "—"} min</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Result</CardTitle></CardHeader>
          <CardContent>
            {scorePct !== null ? (
              <p className={cn("text-lg font-bold flex items-center gap-1", passed ? "text-success" : "text-destructive")}>
                {passed ? <CheckCircleIcon className="size-5" /> : <XCircleIcon className="size-5" />}
                {passed ? "Passed" : "Failed"}
              </p>
            ) : (
              <p className="text-lg font-bold text-muted-foreground">Awaiting grading</p>
            )}
          </CardContent>
        </Card>
      </div>

      {needsManualGrading.length > 0 && (
        <Card className="border-warning/40">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertCircleIcon className="size-5 text-warning" />
              Manual Grading Required ({needsManualGrading.length})
            </CardTitle>
            <CardDescription>These questions need your review before the attempt can be finalized.</CardDescription>
          </CardHeader>
        </Card>
      )}

      {/* Auto-graded questions (read-only) */}
      {autoGraded.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Auto-Graded Answers</CardTitle>
            <CardDescription>Objective questions scored automatically</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {autoGraded.map((q) => {
              const answer = attempt.answers.find((a) => a.questionId === q.id);
              const question = getQuestionById(q.id);
              if (!question || !answer) return null;
              const isCorrect = (answer.score ?? 0) >= q.points;
              return (
                <div key={q.id} className="rounded-lg border p-4 space-y-2">
                  <div className="flex items-start justify-between gap-2">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        <Badge variant="secondary" className="text-xs">{questionTypeLabels[q.type]}</Badge>
                        <span className="text-sm font-medium">{q.title}</span>
                      </div>
                      <p className="text-xs text-muted-foreground">{q.text}</p>
                    </div>
                    <Badge variant={isCorrect ? "success" : "destructive"}>
                      {answer.score ?? 0}/{q.points}
                    </Badge>
                  </div>
                  <Separator />
                  <div className="space-y-1">
                    <Label className="text-xs text-muted-foreground">Student Answer:</Label>
                    <QuestionAnswerRenderer question={question} value={answer.value} onChange={() => {}} readOnly />
                  </div>
                  {question.explanation && (
                    <div className="rounded-md bg-muted/50 p-2 text-xs text-muted-foreground">
                      <span className="font-medium">Explanation: </span>{question.explanation}
                    </div>
                  )}
                </div>
              );
            })}
          </CardContent>
        </Card>
      )}

      {/* Manual grading section */}
      {needsManualGrading.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Manual Grading</CardTitle>
            <CardDescription>Review and assign scores for subjective questions</CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {needsManualGrading.map((q) => {
              const answer = attempt.answers.find((a) => a.questionId === q.id);
              const question = getQuestionById(q.id);
              if (!question || !answer) return null;
              const graded = gradedAnswers[q.id];

              return (
                <div key={q.id} className="rounded-lg border p-4 space-y-3">
                  <div className="flex items-start justify-between gap-2">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        <Badge variant="secondary" className="text-xs">{questionTypeLabels[q.type]}</Badge>
                        <span className="text-sm font-medium">{q.title}</span>
                      </div>
                      <p className="text-xs text-muted-foreground">{q.text}</p>
                    </div>
                    <Badge variant="outline">Max {q.points} pts</Badge>
                  </div>
                  <Separator />
                  <div className="space-y-1">
                    <Label className="text-xs text-muted-foreground">Student Answer:</Label>
                    <QuestionAnswerRenderer question={question} value={answer.value} onChange={() => {}} readOnly />
                  </div>
                  {question.explanation && (
                    <div className="rounded-md bg-muted/50 p-2 text-xs text-muted-foreground">
                      <span className="font-medium">Reference: </span>{question.explanation}
                    </div>
                  )}
                  <Separator />
                  <div className="grid grid-cols-[120px_1fr] gap-4 items-start">
                    <div className="space-y-1">
                      <Label htmlFor={`score-${q.id}`} className="text-xs">Score (0–{q.points})</Label>
                      <Input
                        id={`score-${q.id}`}
                        type="number"
                        min={0}
                        max={q.points}
                        value={graded?.score ?? ""}
                        onChange={(e) => updateScore(q.id, e.target.value)}
                        className="w-24"
                      />
                    </div>
                    <div className="space-y-1">
                      <Label htmlFor={`feedback-${q.id}`} className="text-xs">Feedback (optional)</Label>
                      <Textarea
                        id={`feedback-${q.id}`}
                        value={graded?.feedback ?? ""}
                        onChange={(e) => updateFeedback(q.id, e.target.value)}
                        placeholder="Provide feedback to the student..."
                        className="min-h-[80px]"
                      />
                    </div>
                  </div>
                </div>
              );
            })}
            <div className="flex justify-end">
              <Button onClick={handleSaveGrades}>
                <SaveIcon className="size-4 mr-1" />
                {saved ? "Grades Saved!" : "Save Grades"}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
