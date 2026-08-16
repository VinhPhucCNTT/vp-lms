import * as React from "react";
import { SearchIcon, ClockIcon, CheckCircleIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Textarea } from "@/components/ui/textarea";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { instructorApi, type SubmissionDetailDto } from "@/features/courses/instructor-api";
import { cn } from "@/lib/utils";
import type { SubmissionVerdict } from "@/types";

const verdictColors: Record<string, string> = {
  accepted: "bg-success/20 text-success",
  "wrong-answer": "bg-destructive/20 text-destructive",
  "time-limit-exceeded": "bg-warning/20 text-warning-foreground",
  "memory-limit-exceeded": "bg-warning/20 text-warning-foreground",
  "runtime-error": "bg-destructive/20 text-destructive",
  "compilation-error": "bg-destructive/20 text-destructive",
  pending: "bg-muted text-muted-foreground",
  graded: "bg-success/20 text-success",
};

export function InstructorSubmissions() {
  const { data: allSubmissions, loading, error, reload } = useApi<SubmissionDetailDto[]>(() => instructorApi.getAllSubmissions());
  const [search, setSearch] = React.useState("");
  const [statusFilter, setStatusFilter] = React.useState<string>("all");
  const [selectedSubmission, setSelectedSubmission] = React.useState<SubmissionDetailDto | null>(null);
  const [grade, setGrade] = React.useState("");
  const [feedback, setFeedback] = React.useState("");
  const [submitting, setSubmitting] = React.useState(false);

  const submissions = allSubmissions ?? [];

  const filteredSubmissions = submissions.filter((s) => {
    const matchesSearch = s.student.name.toLowerCase().includes(search.toLowerCase()) || s.assignmentTitle.toLowerCase().includes(search.toLowerCase()) || s.course.code.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === "all" || s.verdict === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const pendingCount = submissions.filter((s) => s.verdict === "pending").length;
  const gradedThisWeek = submissions.filter((s) => s.verdict !== "pending").length;

  const handleGrade = async () => {
    if (!selectedSubmission) return;
    setSubmitting(true);
    try {
      await instructorApi.gradeSubmission(selectedSubmission.id, Number(grade), feedback);
      setSelectedSubmission(null);
      setGrade("");
      setFeedback("");
      reload();
    } catch (err: unknown) {
      console.error("Failed to grade submission:", err);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <LoadingState label="Loading submissions..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <PageHeader title="Submissions" description="Review and grade student submissions" breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Submissions" }]} />

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><ClockIcon className="size-4 text-warning" />Pending Review</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{pendingCount}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><CheckCircleIcon className="size-4 text-success" />Graded This Week</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{gradedThisWeek}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Average Grading Time</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">—</p></CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search by student, assignment..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Tabs value={statusFilter} onValueChange={setStatusFilter}>
          <TabsList>
            <TabsTrigger value="all">All</TabsTrigger>
            <TabsTrigger value="pending">Pending</TabsTrigger>
            <TabsTrigger value="accepted">Graded</TabsTrigger>
          </TabsList>
        </Tabs>
      </div>

      {filteredSubmissions.length === 0 ? (
        <EmptyState message="No submissions found." />
      ) : (
        <div className="space-y-3">
          {filteredSubmissions.map((submission) => (
            <Card key={submission.id} className={cn(submission.verdict === "pending" && "border-warning/50")}>
              <CardContent className="p-4">
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-4">
                    <Avatar><AvatarFallback>{submission.student.name.split(" ").map((n) => n[0]).join("")}</AvatarFallback></Avatar>
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        <p className="font-medium">{submission.student.name}</p>
                        <Badge variant="outline" className="text-xs">{submission.course.code}</Badge>
                      </div>
                      <p className="text-sm text-muted-foreground">{submission.assignmentTitle}</p>
                      <div className="flex items-center gap-3 text-xs text-muted-foreground">
                        <span>{submission.type}</span>
                        <span className="capitalize">{submission.language || "text"}</span>
                        <span>{submission.submittedAt}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <Badge className={cn(verdictColors[submission.verdict] ?? verdictColors.pending)}>{submission.verdict}</Badge>
                    {submission.verdict === "pending" && <Button size="sm" onClick={() => setSelectedSubmission(submission)}>Grade</Button>}
                    {submission.verdict !== "pending" && <Button size="sm" variant="outline" onClick={() => setSelectedSubmission(submission)}>View</Button>}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Dialog open={!!selectedSubmission} onOpenChange={(open) => !open && setSelectedSubmission(null)}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Grade Submission</DialogTitle>
            <DialogDescription>{selectedSubmission?.assignmentTitle} - {selectedSubmission?.student.name}</DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="flex items-center gap-4">
              <Avatar><AvatarFallback>{selectedSubmission?.student.name.split(" ").map((n) => n[0]).join("")}</AvatarFallback></Avatar>
              <div>
                <p className="font-medium">{selectedSubmission?.student.name}</p>
                <p className="text-sm text-muted-foreground">{selectedSubmission?.student.email}</p>
              </div>
            </div>
            <Separator />
            <div>
              <Label className="mb-2 block">Submission Content</Label>
              <div className="rounded-lg border bg-muted p-4">
                {selectedSubmission?.type === "code" ? (
                  <pre className="text-sm font-mono whitespace-pre-wrap">{selectedSubmission?.content}</pre>
                ) : (
                  <p className="text-sm">{selectedSubmission?.content}</p>
                )}
              </div>
            </div>
            {selectedSubmission?.executionTime != null && (
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div><span className="text-muted-foreground">Execution Time: </span><span className="font-medium">{selectedSubmission.executionTime}ms</span></div>
                <div><span className="text-muted-foreground">Memory Used: </span><span className="font-medium">{selectedSubmission.memoryUsed}MB</span></div>
              </div>
            )}
            <Separator />
            <div className="grid gap-4">
              <div className="space-y-2">
                <Label htmlFor="grade">Grade (0-100)</Label>
                <Input id="grade" type="number" placeholder="Enter grade" value={grade} onChange={(e) => setGrade(e.target.value)} min={0} max={100} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="feedback">Feedback</Label>
                <Textarea id="feedback" placeholder="Provide feedback for the student..." value={feedback} onChange={(e) => setFeedback(e.target.value)} rows={4} />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setSelectedSubmission(null)}>Cancel</Button>
            <Button onClick={handleGrade} disabled={submitting}>{submitting ? "Submitting..." : "Submit Grade"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
