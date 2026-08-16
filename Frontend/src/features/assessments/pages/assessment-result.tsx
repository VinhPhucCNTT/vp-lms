import { Link, useParams } from "react-router-dom";
import { ArrowLeftIcon, CheckCircle2Icon, TrophyIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { assessmentApi, type AssessmentAttemptDetailDto } from "@/features/assessments/assessment-api";

export function AssessmentResultPage() {
  const { assessmentId, attemptId } = useParams<{ assessmentId: string; attemptId: string }>();
  const state = useApi<AssessmentAttemptDetailDto>(
    () => assessmentApi.getAttempt(assessmentId ?? "", attemptId ?? ""),
    [assessmentId, attemptId],
  );

  if (state.loading) return <LoadingState label="Loading assessment result..." />;
  if (state.error) return <ErrorState message={state.error} onRetry={state.reload} />;
  if (!state.data) return <ErrorState message="Assessment result is unavailable." onRetry={state.reload} />;

  const attempt = state.data;
  const score = attempt.totalScore ?? 0;
  const percentage = attempt.maxScore > 0 ? Math.round((score / attempt.maxScore) * 100) : 0;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center justify-between gap-4 border-b bg-card px-4 py-3">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/student/assessments"><ArrowLeftIcon className="size-4 mr-1" />Assessments</Link>
        </Button>
        <Badge variant="success">{attempt.status}</Badge>
      </div>

      <Card>
        <CardHeader className="text-center">
          <TrophyIcon className="size-10 mx-auto text-primary" />
          <CardTitle>Assessment Result</CardTitle>
          <p className="text-sm text-muted-foreground">Attempt #{attempt.attemptNumber}</p>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="text-center">
            <p className="text-4xl font-bold">{score}/{attempt.maxScore}</p>
            <p className="text-sm text-muted-foreground">{percentage}%</p>
          </div>
          <div className="grid gap-3 sm:grid-cols-2 text-sm">
            <div className="rounded-lg border p-3">
              <p className="text-muted-foreground">Submitted</p>
              <p className="font-medium">{attempt.submittedAt ? new Date(attempt.submittedAt).toLocaleString() : "—"}</p>
            </div>
            <div className="rounded-lg border p-3">
              <p className="text-muted-foreground">Questions</p>
              <p className="font-medium">{attempt.questions.length}</p>
            </div>
          </div>
          <div className="flex items-center justify-center gap-2 text-sm text-muted-foreground">
            <CheckCircle2Icon className="size-4 text-success" />
            Your score has been saved.
          </div>
          <div className="flex justify-center">
            <Button asChild><Link to="/student/assessments">Back to Assessments</Link></Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
