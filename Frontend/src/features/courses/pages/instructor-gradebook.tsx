import * as React from "react";
import { SearchIcon, DownloadIcon, TrendingUpIcon, TrendingDownIcon } from "lucide-react";
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
import { courses } from "@/shared/data/courses";
import { students } from "@/shared/data/users";
import { cn } from "@/lib/utils";

interface StudentGrade {
  studentId: string;
  studentName: string;
  email: string;
  assignments: { title: string; score: number; maxScore: number }[];
  assessments: { title: string; score: number; maxScore: number }[];
  finalGrade: string;
  percentage: number;
}

const gradeColors: Record<string, string> = {
  A: "text-success",
  B: "text-info",
  C: "text-warning",
  D: "text-warning",
  F: "text-destructive",
};

export function InstructorGradebook() {
  const [search, setSearch] = React.useState("");
  const [selectedCourse, setSelectedCourse] = React.useState("CS 101");
  const [sortBy, setSortBy] = React.useState<string>("name");

  const studentGrades: StudentGrade[] = students.slice(0, 6).map((student, index) => ({
    studentId: student.studentId,
    studentName: `${student.firstName} ${student.lastName}`,
    email: student.email,
    assignments: [
      { title: "A1: Algorithm Analysis", score: Math.floor(Math.random() * 20) + 80, maxScore: 100 },
      { title: "A2: Sorting Implementation", score: Math.floor(Math.random() * 25) + 75, maxScore: 100 },
      { title: "A3: Binary Trees", score: Math.floor(Math.random() * 30) + 70, maxScore: 100 },
    ],
    assessments: [
      { title: "Quiz 1", score: Math.floor(Math.random() * 15) + 85, maxScore: 100 },
      { title: "Midterm", score: Math.floor(Math.random() * 20) + 70, maxScore: 100 },
    ],
    finalGrade: ["A", "A-", "B+", "B", "B-", "C+"][index],
    percentage: Math.floor(Math.random() * 15) + 75,
  }));

  const filteredGrades = studentGrades
    .filter((g) => g.studentName.toLowerCase().includes(search.toLowerCase()) || g.studentId.toLowerCase().includes(search.toLowerCase()))
    .sort((a, b) => {
      if (sortBy === "name") return a.studentName.localeCompare(b.studentName);
      if (sortBy === "percentage") return b.percentage - a.percentage;
      return 0;
    });

  const classAverage = Math.round(studentGrades.reduce((sum, g) => sum + g.percentage, 0) / studentGrades.length);
  const passingRate = Math.round((studentGrades.filter((g) => g.percentage >= 60).length / studentGrades.length) * 100);

  return (
    <div className="space-y-6">
      <PageHeader title="Gradebook" description="View and manage student grades" breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Gradebook" }]} actions={<Button variant="outline"><DownloadIcon className="size-4 mr-2" />Export CSV</Button>} />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Class Average</CardTitle></CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{classAverage}%</p>
            <div className="flex items-center gap-1 text-xs text-success mt-1"><TrendingUpIcon className="size-3" />+3% from last week</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Passing Rate</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{passingRate}%</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">A/B Students</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{studentGrades.filter((g) => g.percentage >= 80).length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Needs Attention</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold text-warning">{studentGrades.filter((g) => g.percentage < 70).length}</p></CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search students..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Select value={selectedCourse} onValueChange={setSelectedCourse}>
          <SelectTrigger className="w-48"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="CS 101">CS 101 - Intro to Algorithms</SelectItem>
            <SelectItem value="CS 201">CS 201 - Operating Systems</SelectItem>
            <SelectItem value="CS 301">CS 301 - Database Systems</SelectItem>
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
          <Card>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Student</TableHead>
                  <TableHead>A1</TableHead>
                  <TableHead>A2</TableHead>
                  <TableHead>A3</TableHead>
                  <TableHead>Quiz 1</TableHead>
                  <TableHead>Midterm</TableHead>
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
                      <TableCell key={i}>
                        <span className={cn(a.score < 70 && "text-destructive", a.score >= 90 && "text-success")}>{a.score}%</span>
                      </TableCell>
                    ))}
                    {student.assessments.map((a, i) => (
                      <TableCell key={i}>
                        <span className={cn(a.score < 70 && "text-destructive", a.score >= 90 && "text-success")}>{a.score}%</span>
                      </TableCell>
                    ))}
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <span className="font-bold">{student.percentage}%</span>
                        <Progress value={student.percentage} className="w-16" />
                      </div>
                    </TableCell>
                    <TableCell><Badge className={cn(gradeColors[student.finalGrade[0]] + " font-bold")}>{student.finalGrade}</Badge></TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Card>
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
                  const count = studentGrades.filter((g) => g.finalGrade[0] === grade).length;
                  const percentage = (count / studentGrades.length) * 100;
                  return (
                    <div key={grade} className="space-y-1">
                      <div className="flex items-center justify-between text-sm">
                        <span className="font-medium">{grade}s</span>
                        <span className="text-muted-foreground">{count} students ({percentage.toFixed(0)}%)</span>
                      </div>
                      <Progress value={percentage} className="h-2" />
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
                {studentGrades.filter((g) => g.percentage < 70).length === 0 ? (
                  <p className="text-sm text-muted-foreground">All students are performing well!</p>
                ) : (
                  <div className="space-y-3">
                    {studentGrades.filter((g) => g.percentage < 70).map((student) => (
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
