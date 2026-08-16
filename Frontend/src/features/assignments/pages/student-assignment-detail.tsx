import * as React from "react";
import { ArrowLeftIcon, CheckCircle2Icon, ClockIcon, FileTextIcon, PaperclipIcon, SendIcon } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import {
  assignmentApi,
  type AssignmentInfoDto,
  type AssignmentResponseDto,
  type SubmissionDetailDto,
} from "@/features/assignments/assignment-api";

function submissionTypeLabel(info: AssignmentInfoDto): string {
  if (info.submissionType === 0 || info.submissionType === "File") return "File submission";
  if (info.submissionType === 1 || info.submissionType === "Text") return "Text submission";
  return "File or text submission";
}

function acceptsFiles(info: AssignmentInfoDto): boolean {
  return info.submissionType === 0 || info.submissionType === 2 || info.submissionType === "File" || info.submissionType === "Both";
}

function acceptsText(info: AssignmentInfoDto): boolean {
  return info.submissionType === 1 || info.submissionType === 2 || info.submissionType === "Text" || info.submissionType === "Both";
}

function formatDate(value: string | null): string {
  return value ? new Date(value).toLocaleString() : "No date set";
}

export function StudentAssignmentDetailPage() {
  const { assignmentId } = useParams<{ assignmentId: string }>();
  const assignmentState = useApi<AssignmentResponseDto>(
    () => assignmentApi.getAssignment(assignmentId ?? ""),
    [assignmentId],
  );
  const submissionState = useApi<SubmissionDetailDto>(
    () => assignmentApi.getOwnSubmission(assignmentId ?? ""),
    [assignmentId],
  );
  const [submissionText, setSubmissionText] = React.useState("");
  const [uploading, setUploading] = React.useState(false);
  const [uploadError, setUploadError] = React.useState<string | null>(null);
  const [submitError, setSubmitError] = React.useState<string | null>(null);
  const [submitting, setSubmitting] = React.useState(false);
  const fileInputRef = React.useRef<HTMLInputElement>(null);

  React.useEffect(() => {
    setSubmissionText(submissionState.data?.submissionText ?? "");
  }, [submissionState.data?.submissionText]);

  if (assignmentState.loading || submissionState.loading) {
    return <LoadingState label="Loading assignment..." />;
  }
  if (assignmentState.error) {
    return <ErrorState message={assignmentState.error} onRetry={assignmentState.reload} />;
  }
  if (submissionState.error) {
    return <ErrorState message={submissionState.error} onRetry={submissionState.reload} />;
  }
  if (!assignmentState.data || !submissionState.data) {
    return <ErrorState message="Assignment details are unavailable." onRetry={assignmentState.reload} />;
  }

  const assignment = assignmentState.data;
  const info = assignment.info;
  const submission = submissionState.data;
  const now = Date.now();
  const upcoming = !!info.openDate && now < new Date(info.openDate).getTime();
  const closed = !!info.closeDate && now >= new Date(info.closeDate).getTime();
  const alreadySubmitted = submission.status === "submitted" || submission.status === "graded";
  const canSubmit = !upcoming && !closed && !alreadySubmitted;
  const maxFilesReached = info.maxFileCount !== null && submission.files.length >= info.maxFileCount;

  const handleFiles = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files ?? []);
    event.target.value = "";
    if (files.length === 0 || !assignmentId) return;

    setUploadError(null);
    setUploading(true);
    try {
      for (const file of files) {
        await assignmentApi.uploadAssignmentFile(assignmentId, file);
      }
      submissionState.reload();
    } catch (error) {
      setUploadError(error instanceof Error ? error.message : "Unable to upload this file.");
    } finally {
      setUploading(false);
    }
  };

  const handleSubmit = async () => {
    if (!assignmentId || !canSubmit) return;
    setSubmitting(true);
    setSubmitError(null);
    try {
      const result = await assignmentApi.submitAssignment(
        assignmentId,
        acceptsText(info) ? submissionText : null,
      );
      setSubmissionText(result.submissionText ?? "");
      submissionState.reload();
    } catch (error) {
      setSubmitError(error instanceof Error ? error.message : "Unable to submit this assignment.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" asChild>
          <Link to="/student/assignments"><ArrowLeftIcon className="size-4 mr-1" />Assignments</Link>
        </Button>
        <Badge variant={submission.status === "graded" ? "success" : alreadySubmitted ? "info" : closed ? "destructive" : "warning"}>
          {submission.status === "not-submitted" ? (closed ? "Closed" : upcoming ? "Upcoming" : "Not submitted") : submission.status}
        </Badge>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">{assignment.resourceInfo.title}</CardTitle>
          <CardDescription className="flex flex-wrap gap-4">
            <span className="flex items-center gap-1.5"><ClockIcon className="size-4" />{closed ? "Closed" : `Due ${formatDate(info.closeDate)}`}</span>
            <span>{submissionTypeLabel(info)}</span>
            {info.maxFileCount !== null && <span>Up to {info.maxFileCount} files</span>}
            {acceptsFiles(info) && <span>Max {info.maxFileSizeKb} KB each</span>}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {upcoming && <p className="rounded-md border border-warning/40 bg-warning/10 p-3 text-sm">This assignment opens {formatDate(info.openDate)}.</p>}
          {closed && <p className="rounded-md border border-destructive/40 bg-destructive/10 p-3 text-sm text-destructive">This assignment is closed for submissions.</p>}

          <div className="rounded-lg border bg-muted/30 p-4 whitespace-pre-wrap text-sm leading-relaxed">
            {info.instructionsMD}
          </div>

          <div className="space-y-4">
            <div>
              <h2 className="font-semibold">Your submission</h2>
              <p className="text-sm text-muted-foreground">{alreadySubmitted ? `Submitted ${formatDate(submission.submittedOn)}` : "Upload your work and submit when ready."}</p>
            </div>

            {acceptsFiles(info) && (
              <div className="space-y-3">
                <div className="rounded-lg border border-dashed p-6 text-center space-y-3">
                  <PaperclipIcon className="size-8 text-muted-foreground mx-auto" />
                  <div>
                    <p className="font-medium text-sm">Attach your work</p>
                    <p className="text-xs text-muted-foreground mt-1">
                      {info.allowedExtensions?.join(", ") || "Allowed file types set by the instructor"}
                    </p>
                  </div>
                  <Input ref={fileInputRef} type="file" multiple={info.maxFileCount !== 1} onChange={handleFiles} className="hidden" disabled={!canSubmit || uploading || maxFilesReached} />
                  <Button variant="outline" size="sm" onClick={() => fileInputRef.current?.click()} disabled={!canSubmit || uploading || maxFilesReached}>
                    {uploading ? "Uploading..." : maxFilesReached ? "File limit reached" : "Choose files"}
                  </Button>
                </div>
                {uploadError && <p className="text-sm text-destructive">{uploadError}</p>}
                {submission.files.length > 0 && (
                  <div className="space-y-2">
                    {submission.files.map((file) => (
                      <a key={file.id} href={assignmentApi.getFileUrl(file.id)} target="_blank" rel="noreferrer" className="flex items-center gap-2 rounded-md border p-3 text-sm hover:bg-muted/50">
                        <FileTextIcon className="size-4 text-muted-foreground" />
                        <span className="flex-1 truncate">{file.originalFileName}</span>
                        <span className="text-xs text-muted-foreground">{Math.ceil(file.sizeInBytes / 1024)} KB</span>
                      </a>
                    ))}
                  </div>
                )}
              </div>
            )}

            {acceptsText(info) && (
              <div className="space-y-2">
                <label htmlFor="submission-text" className="text-sm font-medium">Text response</label>
                <Textarea id="submission-text" value={submissionText} onChange={(event) => setSubmissionText(event.target.value)} disabled={!canSubmit} placeholder="Write your response here..." />
                <p className="text-xs text-muted-foreground">
                  {submissionText.length} characters{info.minTextLength !== null ? ` · minimum ${info.minTextLength}` : ""}{info.maxTextLength !== null ? ` · maximum ${info.maxTextLength}` : ""}
                </p>
              </div>
            )}

            {(submission.score !== null || submission.feedbackText) && (
              <div className="rounded-lg border border-success/30 bg-success/5 p-4 space-y-1">
                {submission.score !== null && <p className="font-medium">Grade: {submission.score}</p>}
                {submission.feedbackText && <p className="text-sm whitespace-pre-wrap">{submission.feedbackText}</p>}
              </div>
            )}

            {submitError && <p className="text-sm text-destructive">{submitError}</p>}
            {!alreadySubmitted && (
              <Button onClick={() => void handleSubmit()} disabled={!canSubmit || submitting || uploading} className="gap-2">
                <SendIcon className="size-4" />
                {submitting ? "Submitting..." : "Submit assignment"}
              </Button>
            )}
            {alreadySubmitted && (
              <div className="flex items-center gap-2 text-sm text-success"><CheckCircle2Icon className="size-4" />Submission saved</div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
