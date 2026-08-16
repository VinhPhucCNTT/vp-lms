import * as React from "react";
import { Link } from "react-router-dom";
import {
  PlusCircleIcon,
  SearchIcon,
  ClockIcon,
  TrophyIcon,
  UsersIcon,
  AlertCircleIcon,
  ClipboardListIcon,
  EyeIcon,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { assessmentApi, type InstructorAssessmentSummaryDto } from "@/features/assessments/assessment-api";

export function InstructorAssessments() {
  const { data: summaries, loading, error, reload } = useApi<InstructorAssessmentSummaryDto[]>(
    () => assessmentApi.getInstructorAssessmentSummaries()
  );
  const [search, setSearch] = React.useState("");
  const [statusFilter, setStatusFilter] = React.useState<string>("all");

  const enriched = summaries ?? [];

  const filtered = enriched.filter((s) => {
    const matchesSearch = s.assessment.title.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === "all" || s.assessment.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const totalQuestions = enriched.reduce((s, a) => s + a.questionCount, 0);
  const totalNeedsGrading = enriched.reduce((s, a) => s + a.needsGrading, 0);
  const totalAttempts = enriched.reduce((s, a) => s + a.attemptCount, 0);

  if (loading) return <LoadingState label="Loading assessments..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Assessments"
        description="Create and manage quizzes and exams across your courses"
        breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Assessments" }]}
        actions={
          <Button asChild>
            <Link to="/instructor/question-banks">
              <PlusCircleIcon className="size-4 mr-2" />New Assessment
            </Link>
          </Button>
        }
      />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><ClipboardListIcon className="size-4" />Assessments</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{enriched.length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><TrophyIcon className="size-4" />Total Questions</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{totalQuestions}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><UsersIcon className="size-4" />Total Attempts</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{totalAttempts}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><AlertCircleIcon className="size-4 text-warning" />Needs Grading</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{totalNeedsGrading}</p></CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search assessments..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Status</SelectItem>
            <SelectItem value="published">Published</SelectItem>
            <SelectItem value="draft">Draft</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {filtered.length === 0 ? (
        <EmptyState message="No assessments found." />
      ) : (
        <Card>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Title</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Questions</TableHead>
                <TableHead>Points</TableHead>
                <TableHead>Duration</TableHead>
                <TableHead>Due Date</TableHead>
                <TableHead>Attempts</TableHead>
                <TableHead>Needs Grading</TableHead>
                <TableHead></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map((s) => (
                <TableRow key={s.assessment.id}>
                  <TableCell className="font-medium">
                    <Link to={`/instructor/assessments/${s.assessment.id}`} className="hover:underline">
                      {s.assessment.title}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <Badge variant={s.assessment.status === "published" ? "success" : "secondary"}>
                      {s.assessment.status ?? "draft"}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{s.questionCount}</TableCell>
                  <TableCell className="text-muted-foreground">{s.totalPoints}</TableCell>
                  <TableCell className="text-muted-foreground">
                    <span className="flex items-center gap-1"><ClockIcon className="size-3" />{s.assessment.duration} min</span>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{s.assessment.dueDate}</TableCell>
                  <TableCell className="text-muted-foreground">{s.attemptCount}</TableCell>
                  <TableCell>
                    {s.needsGrading > 0 ? (
                      <Badge variant="warning">{s.needsGrading}</Badge>
                    ) : (
                      <span className="text-muted-foreground">—</span>
                    )}
                  </TableCell>
                  <TableCell>
                    <Button variant="ghost" size="sm" asChild>
                      <Link to={`/instructor/assessments/${s.assessment.id}`}>
                        <EyeIcon className="size-3.5 mr-1" />Manage
                      </Link>
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}
    </div>
  );
}
