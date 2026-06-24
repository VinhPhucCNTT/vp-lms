import * as React from "react"
import {
  SearchIcon,
  ClockIcon,
  CheckCircleIcon,
  FilterIcon,
} from "lucide-react"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Textarea } from "@/components/ui/textarea"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Separator } from "@/components/ui/separator"
import { PageHeader } from "@/shared/components/page-header"
import { assignments, courses } from "@/shared/data/courses"
import { students } from "@/shared/data/users"
import { submissions } from "@/shared/data/problems"
import { cn } from "@/lib/utils"
import type { Submission, SubmissionVerdict } from "@/types"

interface SubmissionWithDetails extends Submission {
  student: { id: string; name: string; email: string }
  course: { code: string; title: string }
  assignmentTitle: string
}

const verdictColors: Record<SubmissionVerdict, string> = {
  accepted: "bg-success/20 text-success",
  "wrong-answer": "bg-destructive/20 text-destructive",
  "time-limit-exceeded": "bg-warning/20 text-warning-foreground",
  "memory-limit-exceeded": "bg-warning/20 text-warning-foreground",
  "runtime-error": "bg-destructive/20 text-destructive",
  "compilation-error": "bg-destructive/20 text-destructive",
  pending: "bg-muted text-muted-foreground",
}

export function InstructorSubmissions() {
  const [search, setSearch] = React.useState("")
  const [statusFilter, setStatusFilter] = React.useState<string>("all")
  const [selectedSubmission, setSelectedSubmission] =
    React.useState<SubmissionWithDetails | null>(null)
  const [grade, setGrade] = React.useState("")
  const [feedback, setFeedback] = React.useState("")

  const allSubmissions: SubmissionWithDetails[] = [
    ...submissions.map((s) => ({
      ...s,
      student: {
        id: "stu-001",
        name: "Alex Chen",
        email: "alex.chen@university.edu",
      },
      course: { code: "CS 101", title: "Introduction to Algorithms" },
      assignmentTitle: "Algorithm Analysis Practice",
    })),
    {
      id: "sub-006",
      userId: "stu-002",
      assignmentId: "asg-002",
      type: "file",
      content: "Submitted file: sorting_implementation.pdf",
      verdict: "pending",
      submittedAt: "2026-01-15T10:30:00",
      student: {
        id: "stu-002",
        name: "Sarah Johnson",
        email: "sarah.johnson@university.edu",
      },
      course: { code: "CS 201", title: "Operating Systems" },
      assignmentTitle: "Implement Sorting Algorithms",
    },
    {
      id: "sub-007",
      userId: "stu-003",
      assignmentId: "asg-001",
      type: "text",
      content:
        "The time complexity of QuickSort is O(n log n) in the average case and O(n^2) in the worst case...",
      verdict: "pending",
      submittedAt: "2026-01-14T15:45:00",
      student: {
        id: "stu-003",
        name: "Michael Brown",
        email: "michael.brown@university.edu",
      },
      course: { code: "CS 101", title: "Introduction to Algorithms" },
      assignmentTitle: "Algorithm Analysis Practice",
    },
    {
      id: "sub-008",
      userId: "stu-004",
      assignmentId: "asg-002",
      type: "code",
      content:
        "def merge_sort(arr):\n    if len(arr) <= 1:\n        return arr\n    mid = len(arr) // 2\n    return merge(merge_sort(arr[:mid]), merge_sort(arr[mid:]))",
      language: "python",
      verdict: "pending",
      submittedAt: "2026-01-13T09:00:00",
      student: {
        id: "stu-004",
        name: "Emma Davis",
        email: "emma.davis@university.edu",
      },
      course: { code: "CS 201", title: "Operating Systems" },
      assignmentTitle: "Implement Sorting Algorithms",
    },
    {
      id: "sub-009",
      userId: "stu-005",
      assignmentId: "asg-001",
      type: "file",
      content: "Submitted files: analysis.pdf, code.zip",
      verdict: "pending",
      submittedAt: "2026-01-12T14:20:00",
      student: {
        id: "stu-005",
        name: "James Wilson",
        email: "james.wilson@university.edu",
      },
      course: { code: "CS 101", title: "Introduction to Algorithms" },
      assignmentTitle: "Algorithm Analysis Practice",
    },
  ]

  const filteredSubmissions = allSubmissions.filter((s) => {
    const matchesSearch =
      s.student.name.toLowerCase().includes(search.toLowerCase()) ||
      s.assignmentTitle.toLowerCase().includes(search.toLowerCase()) ||
      s.course.code.toLowerCase().includes(search.toLowerCase())
    const matchesStatus = statusFilter === "all" || s.verdict === statusFilter
    return matchesSearch && matchesStatus
  })

  const pendingCount = allSubmissions.filter(
    (s) => s.verdict === "pending"
  ).length
  const gradedThisWeek = allSubmissions.filter(
    (s) => s.verdict !== "pending"
  ).length

  const handleGrade = () => {
    if (selectedSubmission) {
      console.log(
        "Grading submission:",
        selectedSubmission.id,
        "Grade:",
        grade,
        "Feedback:",
        feedback
      )
      setSelectedSubmission(null)
      setGrade("")
      setFeedback("")
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Submissions"
        description="Review and grade student submissions"
        breadcrumbs={[
          { label: "Dashboard", href: "/instructor" },
          { label: "Submissions" },
        ]}
      />

      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-medium">
              <ClockIcon className="size-4 text-warning" />
              Pending Review
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{pendingCount}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-medium">
              <CheckCircleIcon className="size-4 text-success" />
              Graded This Week
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{gradedThisWeek}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">
              Average Grading Time
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">2.3 days</p>
          </CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative max-w-md flex-1">
          <SearchIcon className="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search by student, assignment..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
          />
        </div>
        <Tabs value={statusFilter} onValueChange={setStatusFilter}>
          <TabsList>
            <TabsTrigger value="all">All</TabsTrigger>
            <TabsTrigger value="pending">Pending</TabsTrigger>
            <TabsTrigger value="accepted">Graded</TabsTrigger>
          </TabsList>
        </Tabs>
      </div>

      <div className="space-y-3">
        {filteredSubmissions.map((submission) => (
          <Card
            key={submission.id}
            className={cn(
              submission.verdict === "pending" && "border-warning/50"
            )}
          >
            <CardContent className="p-4">
              <div className="flex items-start justify-between">
                <div className="flex items-start gap-4">
                  <Avatar>
                    <AvatarFallback>
                      {submission.student.name
                        .split(" ")
                        .map((n) => n[0])
                        .join("")}
                    </AvatarFallback>
                  </Avatar>
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <p className="font-medium">{submission.student.name}</p>
                      <Badge variant="outline" className="text-xs">
                        {submission.course.code}
                      </Badge>
                    </div>
                    <p className="text-sm text-muted-foreground">
                      {submission.assignmentTitle}
                    </p>
                    <div className="flex items-center gap-3 text-xs text-muted-foreground">
                      <span>{submission.type}</span>
                      <span className="capitalize">
                        {submission.language || "text"}
                      </span>
                      <span>{submission.submittedAt}</span>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <Badge className={cn(verdictColors[submission.verdict])}>
                    {submission.verdict}
                  </Badge>
                  {submission.verdict === "pending" && (
                    <Button
                      size="sm"
                      onClick={() => setSelectedSubmission(submission)}
                    >
                      Grade
                    </Button>
                  )}
                  {submission.verdict !== "pending" && (
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setSelectedSubmission(submission)}
                    >
                      View
                    </Button>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Dialog
        open={!!selectedSubmission}
        onOpenChange={(open) => !open && setSelectedSubmission(null)}
      >
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Grade Submission</DialogTitle>
            <DialogDescription>
              {selectedSubmission?.assignmentTitle} -{" "}
              {selectedSubmission?.student.name}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="flex items-center gap-4">
              <Avatar>
                <AvatarFallback>
                  {selectedSubmission?.student.name
                    .split(" ")
                    .map((n) => n[0])
                    .join("")}
                </AvatarFallback>
              </Avatar>
              <div>
                <p className="font-medium">
                  {selectedSubmission?.student.name}
                </p>
                <p className="text-sm text-muted-foreground">
                  {selectedSubmission?.student.email}
                </p>
              </div>
            </div>
            <Separator />
            <div>
              <Label className="mb-2 block">Submission Content</Label>
              <div className="rounded-lg border bg-muted p-4">
                {selectedSubmission?.type === "code" ? (
                  <pre className="font-mono text-sm whitespace-pre-wrap">
                    {selectedSubmission?.content}
                  </pre>
                ) : (
                  <p className="text-sm">{selectedSubmission?.content}</p>
                )}
              </div>
            </div>
            {selectedSubmission?.executionTime && (
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-muted-foreground">
                    Execution Time:{" "}
                  </span>
                  <span className="font-medium">
                    {selectedSubmission.executionTime}ms
                  </span>
                </div>
                <div>
                  <span className="text-muted-foreground">Memory Used: </span>
                  <span className="font-medium">
                    {selectedSubmission.memoryUsed}MB
                  </span>
                </div>
              </div>
            )}
            <Separator />
            <div className="grid gap-4">
              <div className="space-y-2">
                <Label htmlFor="grade">Grade (0-100)</Label>
                <Input
                  id="grade"
                  type="number"
                  placeholder="Enter grade"
                  value={grade}
                  onChange={(e) => setGrade(e.target.value)}
                  min={0}
                  max={100}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="feedback">Feedback</Label>
                <Textarea
                  id="feedback"
                  placeholder="Provide feedback for the student..."
                  value={feedback}
                  onChange={(e) => setFeedback(e.target.value)}
                  rows={4}
                />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setSelectedSubmission(null)}
            >
              Cancel
            </Button>
            <Button onClick={handleGrade}>Submit Grade</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
