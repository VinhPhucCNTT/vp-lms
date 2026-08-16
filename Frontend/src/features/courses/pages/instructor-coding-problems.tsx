import * as React from "react";
import { SearchIcon, PlusCircleIcon, CodeIcon, CheckCircleIcon, XCircleIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { courseApi, type CourseDetailDto } from "@/features/courses/course-api";
import { judgeApi } from "@/features/courses/judge-api";
import type { Course, Problem } from "@/types";
import { cn } from "@/lib/utils";

const difficultyColors: Record<string, string> = {
  easy: "bg-success text-success-foreground",
  medium: "bg-warning text-warning-foreground",
  hard: "bg-destructive text-destructive-foreground",
};

export function InstructorCodingProblems() {
  const { data: courses } = useApi<Course[]>(() => courseApi.getInstructorCourses());
  const [search, setSearch] = React.useState("");
  const [selectedCourse, setSelectedCourse] = React.useState<string>("all");

  const instructorCourses = courses ?? [];

  const problemsByCourse = React.useRef<Map<string, Problem[]>>(new Map());
  const [allProblems, setAllProblems] = React.useState<{ problem: Problem; courseCode: string }[]>([]);

  React.useEffect(() => {
    if (!courses || courses.length === 0) return;
    let active = true;
    Promise.all(courses.map((c) => judgeApi.getCourseProblems(c.id).then((probs) => probs.map((p) => ({ problem: p, courseCode: c.code })))))
      .then((results) => {
        if (active) setAllProblems(results.flat());
      })
      .catch(() => { if (active) setAllProblems([]); });
    return () => { active = false; };
  }, [courses]);

  const filteredProblems = allProblems.filter(({ problem, courseCode }) => {
    const matchesSearch = problem.title.toLowerCase().includes(search.toLowerCase()) || problem.tags.some((tag) => tag.toLowerCase().includes(search.toLowerCase()));
    const matchesCourse = selectedCourse === "all" || instructorCourses.find((c) => c.id === selectedCourse)?.code === courseCode;
    return matchesSearch && matchesCourse;
  });

  const totalSubmissions = allProblems.reduce((sum, p) => sum + p.problem.submissionCount, 0);
  const acceptedSubmissions = allProblems.reduce((sum, p) => sum + p.problem.acceptedCount, 0);

  if (!courses) return <LoadingState label="Loading courses..." />;

  return (
    <div className="space-y-6">
      <PageHeader title="Coding Problems" description="Manage coding problems across your courses" breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Coding Problems" }]} actions={
        <Button><PlusCircleIcon className="size-4 mr-2" />New Problem</Button>
      } />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><CodeIcon className="size-4" />Total Problems</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{allProblems.length}</p></CardContent>
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

      {filteredProblems.length === 0 ? (
        <EmptyState message="No coding problems found." />
      ) : (
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
              {filteredProblems.map(({ problem, courseCode }) => {
                const acceptanceRate = problem.submissionCount > 0 ? Math.round((problem.acceptedCount / problem.submissionCount) * 100) : 0;
                return (
                  <TableRow key={problem.id}>
                    <TableCell className="font-medium">{problem.title}</TableCell>
                    <TableCell><Badge variant="outline">{courseCode}</Badge></TableCell>
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
      )}
    </div>
  );
}
