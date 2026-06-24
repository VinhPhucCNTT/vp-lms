import * as React from "react";
import { ClockIcon, CheckCircleIcon, AlertTriangleIcon, TrophyIcon } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import { assessments, courses } from "@/shared/data/courses";
import { students } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";
import { cn } from "@/lib/utils";

interface QuizQuestion {
  id: string;
  question: string;
  options: string[];
  correctIndex: number;
}

const sampleQuestions: QuizQuestion[] = [
  { id: "q1", question: "What is the time complexity of binary search?", options: ["O(n)", "O(log n)", "O(n log n)", "O(1)"], correctIndex: 1 },
  { id: "q2", question: "Which data structure uses LIFO principle?", options: ["Queue", "Stack", "Array", "Tree"], correctIndex: 1 },
  { id: "q3", question: "What is the worst case time complexity of QuickSort?", options: ["O(n)", "O(n log n)", "O(n^2)", "O(log n)"], correctIndex: 2 },
];

export function StudentAssessments() {
  const { user } = useAuth();
  const currentUser = students.find((s) => s.id === user?.id) ?? students[0];
  const [selectedAssessment, setSelectedAssessment] = React.useState<string | null>(null);
  const [currentQuestion, setCurrentQuestion] = React.useState(0);
  const [answers, setAnswers] = React.useState<Record<string, number>>({});
  const [showResults, setShowResults] = React.useState(false);

  const enrolledCourseIds = currentUser.enrolledCourses;

  const userAssessments = assessments.map((a) => {
    const course = courses.find((c) => c.id === "cs-101")!;
    const statuses = ["not-started", "completed", "completed", "overdue"] as const;
    const randomStatus = statuses[Math.floor(Math.random() * statuses.length)];
    return {
      ...a,
      course: { code: course.code, title: course.title },
      status: randomStatus,
      score: randomStatus === "completed" ? Math.floor(Math.random() * 30) + 70 : undefined,
      attemptsUsed: Math.floor(Math.random() * a.maxAttempts),
    };
  });

  const activeAssessment = assessments.find((a) => a.id === selectedAssessment);

  const handleAnswer = (questionId: string, optionIndex: number) => {
    setAnswers((prev) => ({ ...prev, [questionId]: optionIndex }));
  };

  const handleSubmit = () => {
    setShowResults(true);
  };

  const handleClose = () => {
    setSelectedAssessment(null);
    setCurrentQuestion(0);
    setAnswers({});
    setShowResults(false);
  };

  const correctCount = sampleQuestions.filter((q) => answers[q.id] === q.correctIndex).length;
  const score = Math.round((correctCount / sampleQuestions.length) * 100);

  const statusColors = {
    "not-started": "bg-muted text-muted-foreground",
    "in-progress": "bg-info/20 text-info",
    completed: "bg-success/20 text-success",
    overdue: "bg-destructive/20 text-destructive",
  };

  const stats = {
    total: userAssessments.length,
    completed: userAssessments.filter((a) => a.status === "completed").length,
    averageScore: userAssessments.filter((a) => a.score).reduce((sum, a) => sum + (a.score || 0), 0) / Math.max(userAssessments.filter((a) => a.score).length, 1),
    passing: userAssessments.filter((a) => a.score && a.score >= a.passingScore).length,
  };

  return (
    <div className="space-y-6">
      <PageHeader title="My Assessments" description="Take quizzes and track your performance" breadcrumbs={[{ label: "Dashboard", href: "/student" }, { label: "Assessments" }]} />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><ClockIcon className="size-4" />Available</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{userAssessments.filter((a) => a.status === "not-started").length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><CheckCircleIcon className="size-4 text-success" />Completed</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.completed}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><TrophyIcon className="size-4 text-warning" />Average Score</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.averageScore.toFixed(1)}%</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><AlertTriangleIcon className="size-4 text-destructive" />Overdue</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{userAssessments.filter((a) => a.status === "overdue").length}</p></CardContent>
        </Card>
      </div>

      <div className="grid gap-4">
        {userAssessments.map((assessment) => (
          <Card key={assessment.id} className={cn(assessment.status === "overdue" && "border-destructive/50")}>
            <CardContent className="p-4">
              <div className="flex items-start justify-between">
                <div className="space-y-2">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">{assessment.course.code}</Badge>
                    <Badge className={cn(statusColors[assessment.status])}>{assessment.status}</Badge>
                  </div>
                  <h3 className="font-semibold">{assessment.title}</h3>
                  <CardDescription className="line-clamp-1">{assessment.description}</CardDescription>
                  <div className="flex items-center gap-4 text-sm text-muted-foreground">
                    <span className="flex items-center gap-1"><ClockIcon className="size-3" />{assessment.duration} min</span>
                    <span>Passing: {assessment.passingScore}%</span>
                    <span>Attempts: {assessment.attemptsUsed}/{assessment.maxAttempts}</span>
                  </div>
                </div>
                <div className="text-right space-y-2">
                  {assessment.status === "completed" && assessment.score !== undefined && (
                    <div>
                      <p className={cn("text-2xl font-bold", assessment.score >= assessment.passingScore ? "text-success" : "text-destructive")}>{assessment.score}%</p>
                      <p className="text-xs text-muted-foreground">{assessment.score >= assessment.passingScore ? "Passed" : "Failed"}</p>
                    </div>
                  )}
                  <Button size="sm" variant={assessment.status === "not-started" ? "default" : "outline"} onClick={() => setSelectedAssessment(assessment.id)} disabled={assessment.attemptsUsed >= assessment.maxAttempts && assessment.status === "completed"}>
                    {assessment.status === "not-started" ? "Start Quiz" : assessment.status === "overdue" ? "View Details" : "Review Results"}
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Dialog open={!!selectedAssessment && !showResults} onOpenChange={(open) => !open && handleClose()}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{activeAssessment?.title}</DialogTitle>
            <DialogDescription>Answer all questions carefully. Once submitted, you cannot change your answers.</DialogDescription>
          </DialogHeader>
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Question {currentQuestion + 1} of {sampleQuestions.length}</span>
              <Progress value={((currentQuestion + 1) / sampleQuestions.length) * 100} className="w-32" />
            </div>
            <Separator />
            {sampleQuestions[currentQuestion] && (
              <div className="space-y-4">
                <p className="font-medium">{sampleQuestions[currentQuestion].question}</p>
                <div className="space-y-2">
                  {sampleQuestions[currentQuestion].options.map((option, index) => (
                    <button
                      key={index}
                      onClick={() => handleAnswer(sampleQuestions[currentQuestion].id, index)}
                      className={cn(
                        "w-full text-left p-3 rounded-lg border transition-colors",
                        answers[sampleQuestions[currentQuestion].id] === index ? "bg-primary/10 border-primary" : "hover:bg-muted"
                      )}
                    >
                      {option}
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setCurrentQuestion((prev) => Math.max(0, prev - 1))} disabled={currentQuestion === 0}>Previous</Button>
            {currentQuestion < sampleQuestions.length - 1 ? (
              <Button onClick={() => setCurrentQuestion((prev) => prev + 1)}>Next</Button>
            ) : (
              <Button onClick={handleSubmit} disabled={Object.keys(answers).length < sampleQuestions.length}>Submit Quiz</Button>
            )}
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={showResults} onOpenChange={(open) => !open && handleClose()}>
        <DialogContent className="max-w-md text-center">
          <DialogHeader>
            <DialogTitle className={cn(score >= 70 ? "text-success" : "text-destructive")}>{score >= 70 ? "Congratulations!" : "Try Again"}</DialogTitle>
          </DialogHeader>
          <div className="py-6">
            <p className="text-5xl font-bold mb-2">{score}%</p>
            <p className="text-muted-foreground">You answered {correctCount} out of {sampleQuestions.length} questions correctly</p>
          </div>
          <DialogFooter><Button onClick={handleClose} className="w-full">Close</Button></DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
