import * as React from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  AlertCircleIcon,
  ArrowLeftIcon,
  CheckCircle2Icon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ClockIcon,
  FlagIcon,
  SendIcon,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { ScrollArea } from "@/components/ui/scroll-area";
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
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import {
  assessmentApi,
  type AssessmentAttemptDetailDto,
  type AssessmentDetailDto,
} from "@/features/assessments/assessment-api";
import { QuestionAnswerRenderer } from "../components/question-answer-renderer";
import { QuestionNavigator, type NavigatorQuestionState } from "../components/question-navigator";
import { CountdownTimer } from "../components/countdown-timer";
import { cn } from "@/lib/utils";
import type { Question } from "@/types";

function answerPayload(question: Question, value: string | string[]): Record<string, unknown> {
  switch (question.type) {
    case "true-false":
      return { value: value === "true" };
    case "multiple-choice":
      return { selectedOptionId: value };
    case "multiple-select":
      return { selectedOptionIds: Array.isArray(value) ? value : [] };
    default:
      return { value };
  }
}

export function AssessmentAttemptWorkspace() {
  const { assessmentId, attemptId } = useParams<{ assessmentId: string; attemptId: string }>();
  const navigate = useNavigate();
  const assessmentState = useApi<AssessmentDetailDto>(
    () => assessmentApi.getAssessmentDetail(assessmentId ?? ""),
    [assessmentId],
  );
  const attemptState = useApi<AssessmentAttemptDetailDto>(
    () => assessmentApi.getAttempt(assessmentId ?? "", attemptId ?? ""),
    [assessmentId, attemptId],
  );

  const [currentIndex, setCurrentIndex] = React.useState(0);
  const [answers, setAnswers] = React.useState<Record<string, string | string[]>>({});
  const [flags, setFlags] = React.useState<Set<string>>(new Set());
  const [savingQuestionId, setSavingQuestionId] = React.useState<string | null>(null);
  const [saveError, setSaveError] = React.useState<string | null>(null);
  const [submitting, setSubmitting] = React.useState(false);
  const [autoSubmitting, setAutoSubmitting] = React.useState(false);
  const [timeExpired, setTimeExpired] = React.useState(false);
  const [submitError, setSubmitError] = React.useState<string | null>(null);
  const initializedAttempt = React.useRef<string | null>(null);
  const attemptIsEditable = (() => {
    const status = attemptState.data?.status.toLowerCase();
    return status === "inprogress" || status === "in-progress";
  })();

  const questions = React.useMemo(
    () => attemptState.data?.questions.map(assessmentApi.mapAttemptQuestion) ?? [],
    [attemptState.data],
  );

  React.useEffect(() => {
    const attempt = attemptState.data;
    if (!attempt || initializedAttempt.current === attempt.assessmentAttemptSqid) return;

    const restoredAnswers: Record<string, string | string[]> = {};
    const restoredFlags = new Set<string>();
    attempt.questions.forEach((dto) => {
      const value = assessmentApi.mapAttemptAnswer(dto);
      if (value !== undefined) restoredAnswers[dto.attemptQuestionSqid] = value;
      if (dto.isFlagged) restoredFlags.add(dto.attemptQuestionSqid);
    });

    setAnswers(restoredAnswers);
    setFlags(restoredFlags);
    initializedAttempt.current = attempt.assessmentAttemptSqid;
  }, [attemptState.data]);

  const handleSubmit = React.useCallback(async (automatic = false) => {
    if (!assessmentId || !attemptId || (!attemptIsEditable && !automatic)) return;

    setSubmitting(true);
    setAutoSubmitting(automatic);
    setSubmitError(null);
    try {
      await assessmentApi.submitAttempt(assessmentId, attemptId);
      navigate(`/student/assessments/${assessmentId}/results/${attemptId}`);
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : "Unable to submit this assessment.");
    } finally {
      setSubmitting(false);
      setAutoSubmitting(false);
    }
  }, [assessmentId, attemptId, attemptIsEditable, navigate]);

  const handleExpire = React.useCallback(() => {
    setTimeExpired(true);
    void handleSubmit(true);
  }, [handleSubmit]);

  if (assessmentState.loading || attemptState.loading) {
    return <LoadingState label="Loading assessment attempt..." />;
  }
  if (assessmentState.error) {
    return <ErrorState message={assessmentState.error} onRetry={assessmentState.reload} />;
  }
  if (attemptState.error) {
    return <ErrorState message={attemptState.error} onRetry={attemptState.reload} />;
  }
  if (!assessmentState.data || !attemptState.data) {
    return <ErrorState message="Assessment attempt is unavailable." onRetry={attemptState.reload} />;
  }
  if (questions.length === 0) {
    return (
      <div className="flex items-center justify-center h-[60vh]">
        <div className="text-center space-y-3">
          <AlertCircleIcon className="size-10 text-muted-foreground mx-auto" />
          <p className="text-muted-foreground">This assessment has no questions.</p>
          <Button variant="outline" asChild>
            <Link to="/student/assessments">Back to Assessments</Link>
          </Button>
        </div>
      </div>
    );
  }

  const assessment = assessmentApi.mapBackendAssessment(assessmentState.data);
  const attempt = attemptState.data;
  const currentQuestion = questions[currentIndex];
  const currentDto = attempt.questions[currentIndex];
  const isEditable = attemptIsEditable && !timeExpired;

  const isAnswered = (question: Question): boolean => {
    const value = answers[question.id];
    if (value === undefined || value === null) return false;
    if (typeof value === "string") return value.trim().length > 0;
    return value.length > 0;
  };

  const navigatorStates: NavigatorQuestionState[] = questions.map((question) => ({
    questionId: question.id,
    answered: isAnswered(question),
    flagged: flags.has(question.id),
  }));
  const answeredCount = navigatorStates.filter((state) => state.answered).length;
  const flaggedCount = navigatorStates.filter((state) => state.flagged).length;

  const handleAnswer = async (value: string | string[]) => {
    if (!isEditable || !assessmentId || !attemptId || !currentDto) return;

    setAnswers((previous) => ({ ...previous, [currentQuestion.id]: value }));
    setSavingQuestionId(currentQuestion.id);
    setSaveError(null);
    try {
      await assessmentApi.saveAttemptAnswer(
        assessmentId,
        attemptId,
        currentDto.attemptQuestionSqid,
        answerPayload(currentQuestion, value),
      );
    } catch (error) {
      setSaveError(error instanceof Error ? error.message : "Unable to save this answer.");
    } finally {
      setSavingQuestionId(null);
    }
  };

  const toggleFlag = (questionId: string) => {
    setFlags((previous) => {
      const next = new Set(previous);
      if (next.has(questionId)) next.delete(questionId);
      else next.add(questionId);
      return next;
    });
  };

  return (
    <div className="h-[calc(100vh-3.5rem)] flex flex-col overflow-hidden">
      <div className="border-b bg-card shrink-0 px-4 lg:px-6 py-3">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-3 min-w-0">
            <Button variant="ghost" size="sm" asChild>
              <Link to="/student/assessments"><ArrowLeftIcon className="size-4 mr-1" />Exit</Link>
            </Button>
            <Separator orientation="vertical" className="h-6" />
            <div className="min-w-0">
              <h1 className="text-sm font-bold truncate">{assessment.title}</h1>
              <p className="text-xs text-muted-foreground">Attempt #{attempt.attemptNumber}</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            {attemptIsEditable && assessment.duration > 0 && (
              <CountdownTimer
                startTime={attempt.startedAt}
                durationMinutes={assessment.duration}
                onExpire={handleExpire}
              />
            )}
            <Badge variant={isEditable ? "warning" : "outline"}>
              {isEditable ? "In Progress" : attempt.status}
            </Badge>
            {isEditable && (
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button size="sm" disabled={submitting}>
                    <SendIcon className="size-3.5 mr-1" />Submit
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Submit assessment?</AlertDialogTitle>
                    <AlertDialogDescription>
                      You answered {answeredCount} of {questions.length} questions. Unanswered questions will receive zero points. This action cannot be undone.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  {submitError && <p className="text-sm text-destructive px-4">{submitError}</p>}
                  <AlertDialogFooter>
                    <AlertDialogCancel>Keep Working</AlertDialogCancel>
                    <AlertDialogAction onClick={() => void handleSubmit()} disabled={submitting}>
                      {submitting ? "Submitting..." : "Submit Now"}
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            )}
            {timeExpired && submitError && (
              <Button size="sm" variant="outline" onClick={() => void handleSubmit(true)} disabled={submitting}>
                {submitting ? "Retrying..." : "Retry submission"}
              </Button>
            )}
          </div>
        </div>
        {(autoSubmitting || submitError) && (
          <div className={cn("mt-2 text-right text-xs", submitError ? "text-destructive" : "text-muted-foreground")}>
            {submitError ?? "Time expired. Submitting your assessment..."}
          </div>
        )}
      </div>

      <div className="flex-1 flex overflow-hidden">
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
                disabled={!isEditable}
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
                  onChange={(value) => void handleAnswer(value)}
                  disabled={!isEditable || savingQuestionId === currentQuestion.id}
                />
                {savingQuestionId === currentQuestion.id && (
                  <p className="mt-4 text-xs text-muted-foreground">Saving answer...</p>
                )}
                {saveError && <p className="mt-4 text-sm text-destructive">{saveError}</p>}
                {!isEditable && (
                  <p className="mt-4 text-sm text-muted-foreground">This attempt is no longer editable.</p>
                )}
              </CardContent>
            </Card>

            <div className="flex items-center justify-between pt-2">
              <Button variant="outline" onClick={() => setCurrentIndex((index) => Math.max(0, index - 1))} disabled={currentIndex === 0}>
                <ChevronLeftIcon className="size-4 mr-1" />Previous
              </Button>
              <span className="text-xs text-muted-foreground">{answeredCount}/{questions.length} answered</span>
              <Button variant="outline" onClick={() => setCurrentIndex((index) => Math.min(questions.length - 1, index + 1))} disabled={currentIndex === questions.length - 1}>
                Next<ChevronRightIcon className="size-4 ml-1" />
              </Button>
            </div>
          </div>
        </ScrollArea>

        <div className="w-72 border-l bg-card shrink-0 hidden lg:flex flex-col">
          <div className="p-4 border-b">
            <h3 className="text-sm font-semibold mb-1">Question Navigator</h3>
            <p className="text-xs text-muted-foreground">Click any number to jump</p>
          </div>
          <div className="p-4 space-y-4">
            <div className="grid grid-cols-2 gap-2 text-xs">
              <div className="flex items-center gap-1.5"><div className="size-3 rounded bg-primary" /><span className="text-muted-foreground">Answered</span></div>
              <div className="flex items-center gap-1.5"><div className="size-3 rounded bg-muted border" /><span className="text-muted-foreground">Unanswered</span></div>
              <div className="flex items-center gap-1.5"><div className="size-3 rounded bg-warning" /><span className="text-muted-foreground">Flagged</span></div>
              <div className="flex items-center gap-1.5"><div className="size-3 rounded ring-2 ring-ring" /><span className="text-muted-foreground">Current</span></div>
            </div>
            <Separator />
            <QuestionNavigator states={navigatorStates} currentIndex={currentIndex} onJump={setCurrentIndex} />
          </div>
          <div className="mt-auto p-4 border-t space-y-2">
            <div className="flex items-center justify-between text-sm"><span className="text-muted-foreground">Answered</span><span className="font-medium">{answeredCount}/{questions.length}</span></div>
            <div className="flex items-center justify-between text-sm"><span className="text-muted-foreground">Flagged</span><span className="font-medium">{flaggedCount}</span></div>
            <div className="flex items-center justify-between text-sm"><span className="text-muted-foreground flex items-center gap-1"><ClockIcon className="size-3" />Time Limit</span><span className="font-medium">{assessment.duration} min</span></div>
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground"><CheckCircle2Icon className="size-3.5" />Answers save to the server</div>
          </div>
        </div>
      </div>
    </div>
  );
}
