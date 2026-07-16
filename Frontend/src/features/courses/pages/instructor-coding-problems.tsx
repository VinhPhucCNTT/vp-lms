import * as React from "react";
import { Link } from "react-router-dom";
import { SearchIcon, PlusCircleIcon, CodeIcon, CheckCircleIcon, XCircleIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { PageHeader } from "@/shared/components/page-header";
import { problems, submissions } from "@/shared/data/problems";
import { courses, courseActivities } from "@/shared/data/courses";
import { instructors } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";
import { cn } from "@/lib/utils";

const difficultyColors: Record<string, string> = {
  easy: "bg-success text-success-foreground",
  medium: "bg-warning text-warning-foreground",
  hard: "bg-destructive text-destructive-foreground",
};

export function InstructorCodingProblems() {
  const { user } = useAuth();
  const currentInstructor = instructors.find((i) => i.id === user?.id) ?? instructors[0];
  const instructorCourses = courses.filter((c) => c.instructorId === currentInstructor.id);
  const [search, setSearch] = React.useState("");
  const [selectedCourse, setSelectedCourse] = React.useState<string>("all");

  const instructorCourseIds = instructorCourses.map((c) => c.id);
  const instructorProblemActivities = courseActivities.filter(
    (a) => a.type === "coding-problem" && instructorCourseIds.includes(a.courseId)
  );

  const instructorProblemIds = instructorProblemActivities.map((a) => a.refId);
  const instructorProblems = problems.filter((p) => instructorProblemIds.includes(p.id));

  const filteredProblems = instructorProblems.filter((problem) => {
    const activity = instructorProblemActivities.find((a) => a.refId === problem.id);
    const matchesSearch = problem.title.toLowerCase().includes(search.toLowerCase()) || problem.tags.some((tag) => tag.toLowerCase().includes(search.toLowerCase()));
    const matchesCourse = selectedCourse === "all" || activity?.courseId === selectedCourse;
    return matchesSearch && matchesCourse;
  });

  const totalSubmissions = submissions.filter((s) => instructorProblemIds.includes(s.problemId)).length;
  const acceptedSubmissions = submissions.filter((s) => instructorProblemIds.includes(s.problemId) && s.verdict === "accepted").length;

  return (
    <div className="space-y-6">
      <PageHeader title="Coding Problems" description="Manage coding problems across your courses" breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Coding Problems" }]} actions={
        <Button><PlusCircleIcon className="size-4 mr-2" />New Problem</Button>
      } />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><CodeIcon className="size-4" />Total Problems</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{instructorProblems.length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><CheckCircleIcon className="size-4 text-success" />Accepted</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{acceptedSubmissions}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><XCircleIcon className="size-4 text-destructive" />Rejected</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{totalSubmissions - acceptedSubmissions}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Acceptance Rate</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{totalSubmissions > 0 ? Math.round((acceptedSubmissions / totalSubmissions) * 100) : 0}%</p></CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search problems..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Select value={selectedCourse} onValueChange={setSelectedCourse}>
          <SelectTrigger className="w-64"><SelectValue placeholder="Filter by course" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Courses</SelectItem>
            {instructorCourses.map((c) => (<SelectItem key={c.id} value={c.id}>{c.code} - {c.title}</SelectItem>))}
          </SelectContent>
        </Select>
      </div>

      <Card>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Title</TableHead>
              <TableHead>Course</TableHead>
              <TableHead>Difficulty</TableHead>
              <TableHead>Submissions</TableHead>
              <TableHead>Acceptance</TableHead>
              <TableHead>Tags</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredProblems.map((problem) => {
              const activity = instructorProblemActivities.find((a) => a.refId === problem.id);
              const course = courses.find((c) => c.id === activity?.courseId);
              const acceptanceRate = Math.round((problem.acceptedCount / problem.submissionCount) * 100);
              return (
                <TableRow key={problem.id}>
                  <TableCell className="font-medium">{problem.title}</TableCell>
                  <TableCell><Badge variant="outline">{course?.code ?? "—"}</Badge></TableCell>
                  <TableCell><Badge className={cn(difficultyColors[problem.difficulty])}>{problem.difficulty}</Badge></TableCell>
                  <TableCell className="text-muted-foreground">{problem.submissionCount}</TableCell>
                  <TableCell className="text-muted-foreground">{acceptanceRate}%</TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      {problem.tags.slice(0, 2).map((tag) => (
                        <Badge key={tag} variant="outline" className="text-xs">{tag}</Badge>
                      ))}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </Card>
    </div>
  );
}
