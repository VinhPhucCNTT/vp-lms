import * as React from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import {
  FlagIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  SendIcon,
  AlertCircleIcon,
  ClockIcon,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { ScrollArea } from "@/components/ui/scroll-area";
import { assessments, courses } from "@/shared/data/courses";
import { getQuestionsByAssessment } from "@/shared/data/question-bank";
import { QuestionAnswerRenderer } from "../components/question-answer-renderer";
import { QuestionNavigator, type NavigatorQuestionState } from "../components/question-navigator";
import { CountdownTimer } from "../components/countdown-timer";
import { cn } from "@/lib/utils";
import type { AttemptAnswer, Question } from "@/types";

export function ActiveAssessment() {
  const { assessmentId } = useParams<{ assessmentId: string }>();
  const navigate = useNavigate();

  const assessment = assessments.find((a) => a.id === assessmentId);
  const questions = assessment ? getQuestionsByAssessment(assessment.id) : [];
  const course = assessment ? courses.find((c) => c.id === "cs-101") : null;

  const startTime = React.useMemo(() => new Date().toISOString(), []);

  const [currentIndex, setCurrentIndex] = React.useState(0);
  const [answers, setAnswers] = React.useState<Record<string, string | string[]>>({});
  const [flags, setFlags] = React.useState<Set<string>>(new Set());
  const [submitted, setSubmitted] = React.useState(false);

  if (!assessment || questions.length === 0) {
    return (
      <div className="flex items-center justify-center h-[60vh]">
        <div className="text-center space-y-3">
          <AlertCircleIcon className="size-10 text-muted-foreground mx-auto" />
          <p className="text-muted-foreground">Assessment not found or has no questions.</p>
          <Button asChild variant="outline">
            <Link to="/student/assessments">Back to Assessments</Link>
          </Button>
        </div>
      </div>
    );
  }

  const currentQuestion: Question = questions[currentIndex];

  const isAnswered = (q: Question): boolean => {
    const v = answers[q.id];
    if (v === undefined || v === null) return false;
    if (typeof v === "string") return v.trim().length > 0;
    return Array.isArray(v) && v.length > 0;
  };

  const navigatorStates: NavigatorQuestionState[] = questions.map((q) => ({
    questionId: q.id,
    answered: isAnswered(q),
    flagged: flags.has(q.id),
  }));

  const answeredCount = navigatorStates.filter((s) => s.answered).length;
  const flaggedCount = navigatorStates.filter((s) => s.flagged).length;

  const setAnswer = (questionId: string, value: string | string[]) => {
    setAnswers((prev) => ({ ...prev, [questionId]: value }));
  };

  const toggleFlag = (questionId: string) => {
    setFlags((prev) => {
      const next = new Set(prev);
      if (next.has(questionId)) next.delete(questionId);
      else next.add(questionId);
      return next;
    });
  };

  const handleSubmit = () => {
    setSubmitted(true);
    navigate(`/student/assessments/${assessment.id}/results`);
  };

  const goPrev = () => setCurrentIndex((i) => Math.max(0, i - 1));
  const goNext = () => setCurrentIndex((i) => Math.min(questions.length - 1, i + 1));

  return (
    <div className="h-[calc(100vh-3.5rem)] flex flex-col overflow-hidden">
      {/* Top bar */}
      <div className="border-b bg-card shrink-0 px-4 lg:px-6 py-3">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-3 min-w-0">
            <Button variant="ghost" size="sm" asChild>
              <Link to="/student/assessments">
                <ChevronLeftIcon className="size-4 mr-1" />Exit
              </Link>
            </Button>
            <Separator orientation="vertical" className="h-6" />
            <div className="min-w-0">
              <h1 className="text-sm font-bold truncate">{assessment.title}</h1>
              {course && (
                <p className="text-xs text-muted-foreground">{course.code} · {course.title}</p>
              )}
            </div>
          </div>
          <div className="flex items-center gap-3 shrink-0">
            <CountdownTimer startTime={startTime} durationMinutes={assessment.duration} onExpire={handleSubmit} />
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button size="sm" disabled={submitted}>
                  <SendIcon className="size-3.5 mr-1" />Submit
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Submit Assessment?</AlertDialogTitle>
                  <AlertDialogDescription>
                    You answered {answeredCount} of {questions.length} questions
                    {flaggedCount > 0 && `, flagged ${flaggedCount}`}.
                    {answeredCount < questions.length && " Unanswered questions will score zero."}
                    {" "}This action cannot be undone.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Keep Working</AlertDialogCancel>
                  <AlertDialogAction onClick={handleSubmit}>Submit Now</AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          </div>
        </div>
      </div>

      {/* Body: two columns */}
      <div className="flex-1 flex overflow-hidden">
        {/* Main question area */}
        <ScrollArea className="flex-1">
          <div className="max-w-3xl mx-auto px-6 lg:px-10 py-6 space-y-6">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-muted-foreground">
                  Question {currentIndex + 1} of {questions.length}
                </span>
                <Badge variant="outline">{currentQuestion.points} pts</Badge>
              </div>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => toggleFlag(currentQuestion.id)}
                className={cn(flags.has(currentQuestion.id) && "text-warning-foreground")}
              >
                <FlagIcon className={cn("size-3.5 mr-1", flags.has(currentQuestion.id) && "fill-current")} />
                {flags.has(currentQuestion.id) ? "Flagged" : "Flag"}
              </Button>
            </div>

            <Card>
              <CardContent className="p-6">
                <QuestionAnswerRenderer
                  question={currentQuestion}
                  value={answers[currentQuestion.id] ?? (currentQuestion.type === "multiple-select" ? [] : "")}
                  onChange={(v) => setAnswer(currentQuestion.id, v)}
                />
              </CardContent>
            </Card>

            <div className="flex items-center justify-between pt-2">
              <Button variant="outline" onClick={goPrev} disabled={currentIndex === 0}>
                <ChevronLeftIcon className="size-4 mr-1" />Previous
              </Button>
              <span className="text-xs text-muted-foreground">
                {answeredCount}/{questions.length} answered
              </span>
              <Button variant="outline" onClick={goNext} disabled={currentIndex === questions.length - 1}>
                Next<ChevronRightIcon className="size-4 ml-1" />
              </Button>
            </div>
          </div>
        </ScrollArea>

        {/* Right sidebar: navigator */}
        <div className="w-72 border-l bg-card shrink-0 hidden lg:flex flex-col">
          <div className="p-4 border-b">
            <h3 className="text-sm font-semibold mb-1">Question Navigator</h3>
            <p className="text-xs text-muted-foreground">Click any number to jump</p>
          </div>
          <div className="p-4 space-y-4">
            <div className="grid grid-cols-2 gap-2 text-xs">
              <div className="flex items-center gap-1.5">
                <div className="size-3 rounded bg-primary" />
                <span className="text-muted-foreground">Answered</span>
              </div>
              <div className="flex items-center gap-1.5">
                <div className="size-3 rounded bg-muted border" />
                <span className="text-muted-foreground">Unanswered</span>
              </div>
              <div className="flex items-center gap-1.5">
                <div className="size-3 rounded bg-warning" />
                <span className="text-muted-foreground">Flagged</span>
              </div>
              <div className="flex items-center gap-1.5">
                <div className="size-3 rounded ring-2 ring-ring" />
                <span className="text-muted-foreground">Current</span>
              </div>
            </div>
            <Separator />
            <QuestionNavigator
              states={navigatorStates}
              currentIndex={currentIndex}
              onJump={setCurrentIndex}
            />
          </div>
          <div className="mt-auto p-4 border-t space-y-2">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Answered</span>
              <span className="font-medium">{answeredCount}/{questions.length}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Flagged</span>
              <span className="font-medium">{flaggedCount}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground flex items-center gap-1">
                <ClockIcon className="size-3" />Time Limit
              </span>
              <span className="font-medium">{assessment.duration} min</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
