import * as React from "react";
import { SearchIcon, DownloadIcon, TrendingUpIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Progress } from "@/components/ui/progress";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { instructorApi, type GradebookStudentDto } from "@/features/courses/instructor-api";
import { courseApi } from "@/features/courses/course-api";
import type { Course } from "@/types";
import { cn } from "@/lib/utils";

const gradeColors: Record<string, string> = {
  A: "text-success",
  B: "text-info",
  C: "text-warning",
  D: "text-warning",
  F: "text-destructive",
};

export function InstructorGradebook() {
  const { data: courses } = useApi<Course[]>(() => courseApi.getInstructorCourses());
  const [search, setSearch] = React.useState("");
  const [selectedCourseId, setSelectedCourseId] = React.useState<string>("");
  const [sortBy, setSortBy] = React.useState<string>("name");

  React.useEffect(() => {
    if (courses && courses.length > 0 && !selectedCourseId) {
      setSelectedCourseId(courses[0].id);
    }
  }, [courses, selectedCourseId]);

  const { data: studentGrades, loading, error, reload } = useApi<GradebookStudentDto[]>(
    () => selectedCourseId ? instructorApi.getGradebook(selectedCourseId) : Promise.resolve([]),
    [selectedCourseId],
  );

  const grades = studentGrades ?? [];

  const filteredGrades = grades
    .filter((g) => g.studentName.toLowerCase().includes(search.toLowerCase()) || g.studentId.toLowerCase().includes(search.toLowerCase()))
    .sort((a, b) => {
      if (sortBy === "name") return a.studentName.localeCompare(b.studentName);
      if (sortBy === "percentage") return b.percentage - a.percentage;
      return 0;
    });

  const classAverage = grades.length > 0 ? Math.round(grades.reduce((sum, g) => sum + g.percentage, 0) / grades.length) : 0;
  const passingRate = grades.length > 0 ? Math.round((grades.filter((g) => g.percentage >= 60).length / grades.length) * 100) : 0;

  if (loading && !grades.length) return <LoadingState label="Loading gradebook..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <PageHeader title="Gradebook" description="View and manage student grades" breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Gradebook" }]} actions={<Button variant="outline"><DownloadIcon className="size-4 mr-2" />Export CSV</Button>} />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Class Average</CardTitle></CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{classAverage}%</p>
            <div className="flex items-center gap-1 text-xs text-success mt-1"><TrendingUpIcon className="size-3" />—</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Passing Rate</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{passingRate}%</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">A/B Students</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{grades.filter((g) => g.percentage >= 80).length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Needs Attention</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold text-warning">{grades.filter((g) => g.percentage < 70).length}</p></CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search students..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Select value={selectedCourseId} onValueChange={setSelectedCourseId}>
          <SelectTrigger className="w-48"><SelectValue placeholder="Select course" /></SelectTrigger>
          <SelectContent>
            {(courses ?? []).map((c) => (<SelectItem key={c.id} value={c.id}>{c.code} - {c.title}</SelectItem>))}
          </SelectContent>
        </Select>
        <Select value={sortBy} onValueChange={setSortBy}>
          <SelectTrigger className="w-40"><SelectValue placeholder="Sort by" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="name">Name</SelectItem>
            <SelectItem value="percentage">Percentage</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Tabs defaultValue="table">
        <TabsList>
          <TabsTrigger value="table">Table View</TabsTrigger>
          <TabsTrigger value="overview">Overview</TabsTrigger>
        </TabsList>

        <TabsContent value="table" className="mt-6">
          {filteredGrades.length === 0 ? (
            <EmptyState message="No grade data available for this course." />
          ) : (
            <Card>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Student</TableHead>
                    {grades[0]?.assignments.map((a, i) => (<TableHead key={i}>{a.title}</TableHead>))}
                    {grades[0]?.assessments.map((a, i) => (<TableHead key={i}>{a.title}</TableHead>))}
                    <TableHead>Total</TableHead>
                    <TableHead>Grade</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {filteredGrades.map((student) => (
                    <TableRow key={student.studentId}>
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <Avatar size="sm"><AvatarFallback>{student.studentName.split(" ").map((n) => n[0]).join("")}</AvatarFallback></Avatar>
                          <div>
                            <p className="font-medium">{student.studentName}</p>
                            <p className="text-xs text-muted-foreground">{student.studentId}</p>
                          </div>
                        </div>
                      </TableCell>
                      {student.assignments.map((a, i) => (
                        <TableCell key={i}><span className={cn(a.score < 70 && "text-destructive", a.score >= 90 && "text-success")}>{a.score}%</span></TableCell>
                      ))}
                      {student.assessments.map((a, i) => (
                        <TableCell key={i}><span className={cn(a.score < 70 && "text-destructive", a.score >= 90 && "text-success")}>{a.score}%</span></TableCell>
                      ))}
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <span className="font-bold">{student.percentage}%</span>
                          <Progress value={student.percentage} className="w-16" />
                        </div>
                      </TableCell>
                      <TableCell><Badge className={cn((gradeColors[student.finalGrade[0]] ?? "") + " font-bold")}>{student.finalGrade}</Badge></TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="overview" className="mt-6">
          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Grade Distribution</CardTitle>
                <CardDescription>Distribution of final grades</CardDescription>
              </CardHeader>
              <CardContent className="space-y-3">
                {["A", "B", "C", "D", "F"].map((grade) => {
                  const count = grades.filter((g) => g.finalGrade[0] === grade).length;
                  const pct = grades.length > 0 ? (count / grades.length) * 100 : 0;
                  return (
                    <div key={grade} className="space-y-1">
                      <div className="flex items-center justify-between text-sm">
                        <span className="font-medium">{grade}s</span>
                        <span className="text-muted-foreground">{count} students ({pct.toFixed(0)}%)</span>
                      </div>
                      <Progress value={pct} className="h-2" />
                    </div>
                  );
                })}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Students Needing Attention</CardTitle>
                <CardDescription>Students below 70% average</CardDescription>
              </CardHeader>
              <CardContent>
                {grades.filter((g) => g.percentage < 70).length === 0 ? (
                  <p className="text-sm text-muted-foreground">All students are performing well!</p>
                ) : (
                  <div className="space-y-3">
                    {grades.filter((g) => g.percentage < 70).map((student) => (
                      <div key={student.studentId} className="flex items-center justify-between p-3 rounded-lg bg-muted">
                        <div className="flex items-center gap-3">
                          <Avatar size="sm"><AvatarFallback>{student.studentName.split(" ").map((n) => n[0]).join("")}</AvatarFallback></Avatar>
                          <div>
                            <p className="font-medium">{student.studentName}</p>
                            <p className="text-xs text-muted-foreground">{student.percentage}% average</p>
                          </div>
                        </div>
                        <Button size="sm" variant="outline">Contact</Button>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
