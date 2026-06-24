import * as React from "react";
import { Link } from "react-router-dom";
import { SearchIcon, TrendingUpIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/shared/components/page-header";
import { problems } from "@/shared/data/problems";
import { cn } from "@/lib/utils";

const difficultyColors: Record<string, string> = {
  easy: "bg-success text-success-foreground",
  medium: "bg-warning text-warning-foreground",
  hard: "bg-destructive text-destructive-foreground",
};

export function ProblemList() {
  const [search, setSearch] = React.useState("");
  const [difficultyFilter, setDifficultyFilter] = React.useState<string>("all");

  const filteredProblems = problems.filter((problem) => {
    const matchesSearch = problem.title.toLowerCase().includes(search.toLowerCase()) || problem.tags.some((tag) => tag.toLowerCase().includes(search.toLowerCase()));
    const matchesDifficulty = difficultyFilter === "all" || problem.difficulty === difficultyFilter;
    return matchesSearch && matchesDifficulty;
  });

  return (
    <div className="space-y-6">
      <PageHeader title="Online Judge" description="Practice coding problems and improve your algorithmic skills" breadcrumbs={[{ label: "Dashboard", href: "/student" }, { label: "Online Judge" }]} />

      <div className="grid gap-4 md:grid-cols-4">
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Total Problems</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold">{problems.length}</p></CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Solved</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold text-success">3</p></CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Attempted</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold text-warning">5</p></CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-1"><TrendingUpIcon className="size-4" />Success Rate</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold">42%</p></CardContent></Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search problems by title or tag..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Tabs value={difficultyFilter} onValueChange={setDifficultyFilter}>
          <TabsList>
            <TabsTrigger value="all">All</TabsTrigger>
            <TabsTrigger value="easy">Easy</TabsTrigger>
            <TabsTrigger value="medium">Medium</TabsTrigger>
            <TabsTrigger value="hard">Hard</TabsTrigger>
          </TabsList>
        </Tabs>
      </div>

      <Card>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-16">Status</TableHead>
              <TableHead>Title</TableHead>
              <TableHead>Difficulty</TableHead>
              <TableHead>Acceptance</TableHead>
              <TableHead>Tags</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredProblems.map((problem) => {
              const acceptanceRate = Math.round((problem.acceptedCount / problem.submissionCount) * 100);
              const solved = Math.random() > 0.5;
              return (
                <TableRow key={problem.id}>
                  <TableCell>{solved ? <span className="text-success">✓</span> : <span className="text-muted-foreground">○</span>}</TableCell>
                  <TableCell>
                    <Link to={`/student/judge/${problem.id}`} className="font-medium hover:text-primary transition-colors">{problem.title}</Link>
                  </TableCell>
                  <TableCell><Badge className={cn(difficultyColors[problem.difficulty])}>{problem.difficulty}</Badge></TableCell>
                  <TableCell className="text-muted-foreground">{acceptanceRate}%</TableCell>
                  <TableCell>
                    <div className="flex flex-wrap gap-1">
                      {problem.tags.slice(0, 3).map((tag) => (
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
