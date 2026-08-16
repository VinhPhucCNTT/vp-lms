import * as React from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ArrowLeftIcon, CalendarIcon, CheckCircle2Icon, ClockIcon, PlayIcon, ZapIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { assessmentApi, type AssessmentDetailDto } from "@/features/assessments/assessment-api";

export function AssessmentDetailPage() {
  const { assessmentId } = useParams<{ assessmentId: string }>();
  const navigate = useNavigate();
  const { data, loading, error, reload } = useApi<AssessmentDetailDto>(
    () => assessmentApi.getAssessmentDetail(assessmentId ?? ""),
    [assessmentId],
  );
  const [starting, setStarting] = React.useState(false);
  const [startError, setStartError] = React.useState<string | null>(null);

  if (loading) return <LoadingState label="Loading assessment..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;
  if (!data) return <ErrorState message="Assessment details are unavailable." onRetry={reload} />;

  const assessment = assessmentApi.mapBackendAssessment(data);
  const now = Date.now();
  const upcoming = !!assessment.availableFrom && now < new Date(assessment.availableFrom).getTime();
  const closed = !!assessment.availableTo && now >= new Date(assessment.availableTo).getTime();
  const unavailable = upcoming || closed;

  const start = async () => {
    setStarting(true);
    setStartError(null);
    try {
      const attempt = await assessmentApi.startAssessment(assessment.id);
      navigate(`/student/assessments/${assessment.id}/attempts/${attempt.assessmentAttemptSqid}`);
    } catch (err: unknown) {
      setStartError(err instanceof Error ? err.message : "Unable to start this assessment.");
    } finally {
      setStarting(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/student/assessments"><ArrowLeftIcon className="size-4 mr-1" />Back</Link>
        </Button>
        <Badge variant={unavailable ? "outline" : "info"}>{upcoming ? "Upcoming" : closed ? "Closed" : "Available"}</Badge>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">{assessment.title}</CardTitle>
          {assessment.description && <p className="text-sm text-muted-foreground">{assessment.description}</p>}
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex flex-wrap gap-4 text-sm text-muted-foreground">
            <span className="flex items-center gap-1.5"><ClockIcon className="size-4" />{assessment.duration} min</span>
            <span className="flex items-center gap-1.5"><ZapIcon className="size-4" />{assessment.maxAttempts} attempt{assessment.maxAttempts === 1 ? "" : "s"}</span>
            {assessment.dueDate && <span className="flex items-center gap-1.5"><CalendarIcon className="size-4" />Available until {new Date(assessment.dueDate).toLocaleString()}</span>}
          </div>

          {assessment.availableFrom && upcoming && (
            <p className="text-sm text-muted-foreground">Available from {new Date(assessment.availableFrom).toLocaleString()}.</p>
          )}
          {startError && <p className="text-sm text-destructive">{startError}</p>}

          <Button onClick={start} disabled={starting || unavailable} className="gap-2">
            {unavailable ? <CheckCircle2Icon className="size-4" /> : <PlayIcon className="size-4" />}
            {starting ? "Starting..." : unavailable ? "Not Available" : "Start Assessment"}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
