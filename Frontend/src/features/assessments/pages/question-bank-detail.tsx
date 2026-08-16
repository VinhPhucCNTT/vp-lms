import * as React from "react";
import { useParams, Link } from "react-router-dom";
import {
  ArrowLeftIcon,
  PlusIcon,
  Trash2Icon,
  Share2Icon,
  LockIcon,
  EditIcon,
  AlertCircleIcon,
  EyeIcon,
  EyeOffIcon,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import {
  questionBanks,
  getQuestionsByBank,
  questionTypeLabels,
  getBankById,
} from "@/shared/data/question-bank";
import { instructors, students } from "@/shared/data/users";
import { QuestionAnswerRenderer, questionTypeIconMap } from "../components/question-answer-renderer";
import { useAuth } from "@/features/auth/auth-context";
import { cn } from "@/lib/utils";

export function QuestionBankDetail() {
  const { bankId } = useParams<{ bankId: string }>();
  const { user } = useAuth();
  const currentInstructor = instructors.find((i) => i.id === user?.id) ?? instructors[0];
  const bank = bankId ? getBankById(bankId) : undefined;
  const bankQuestions = bank ? getQuestionsByBank(bank.id) : [];
  const [expandedQ, setExpandedQ] = React.useState<Set<string>>(new Set());

  if (!bank) {
    return (
      <div className="flex items-center justify-center h-[60vh]">
        <div className="text-center space-y-3">
          <AlertCircleIcon className="size-10 text-muted-foreground mx-auto" />
          <p className="text-muted-foreground">Question bank not found.</p>
          <Button asChild variant="outline">
            <Link to="/instructor/question-banks">Back to Question Banks</Link>
          </Button>
        </div>
      </div>
    );
  }

  const isOwner = bank.ownerId === currentInstructor.id;
  const owner = instructors.find((i) => i.id === bank.ownerId);

  const toggleExpand = (qid: string) => {
    setExpandedQ((prev) => {
      const next = new Set(prev);
      if (next.has(qid)) next.delete(qid);
      else next.add(qid);
      return next;
    });
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title={bank.name}
        description={bank.description}
        breadcrumbs={[
          { label: "Dashboard", href: "/instructor" },
          { label: "Question Banks", href: "/instructor/question-banks" },
          { label: bank.name },
        ]}
        actions={
          <Button variant="outline" asChild>
            <Link to="/instructor/question-banks">
              <ArrowLeftIcon className="size-4 mr-2" />Back
            </Link>
          </Button>
        }
      />

      {/* Bank info banner */}
      <Card>
        <CardContent className="flex items-center justify-between p-4">
          <div className="flex items-center gap-3">
            <div className={cn(
              "flex size-10 items-center justify-center rounded-lg",
              isOwner ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"
            )}>
              {isOwner ? <EditIcon className="size-5" /> : <LockIcon className="size-5" />}
            </div>
            <div>
              <p className="text-sm font-medium">
                {isOwner ? "You own this bank" : `Shared by ${owner?.firstName} ${owner?.lastName}`}
              </p>
              <p className="text-xs text-muted-foreground">
                {isOwner
                  ? "You can add, edit, and share questions in this bank."
                  : "You have read-only access to questions in this bank."}
              </p>
            </div>
          </div>
          {isOwner && (
            <Button variant="outline">
              <Share2Icon className="size-4 mr-2" />Share
            </Button>
          )}
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Questions</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{bankQuestions.length}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Total Points</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{bankQuestions.reduce((s, q) => s + q.points, 0)}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Shared With</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{bank.sharedWithInstructorIds.length + bank.sharedWithCourseIds.length}</p></CardContent>
        </Card>
      </div>

      {/* Questions list */}
      <Card>
        <CardHeader className="flex-row items-center justify-between">
          <div>
            <CardTitle>Questions ({bankQuestions.length})</CardTitle>
            <CardDescription>
              {isOwner ? "Add, edit, and manage questions in this bank" : "View questions in this shared bank"}
            </CardDescription>
          </div>
          {isOwner && (
            <Button>
              <PlusIcon className="size-4 mr-1" />New Question
            </Button>
          )}
        </CardHeader>
        <CardContent>
          {bankQuestions.length === 0 ? (
            <div className="py-12 text-center space-y-3">
              <AlertCircleIcon className="size-10 text-muted-foreground mx-auto" />
              <p className="text-sm text-muted-foreground">No questions in this bank yet.</p>
              {isOwner && (
                <Button>
                  <PlusIcon className="size-4 mr-1" />Add First Question
                </Button>
              )}
            </div>
          ) : (
            <div className="space-y-2">
              {bankQuestions.map((q) => {
                const Icon = questionTypeIconMap[q.type];
                const isExpanded = expandedQ.has(q.id);
                return (
                  <div key={q.id} className="rounded-lg border">
                    <div className="flex items-start gap-3 p-3">
                      <div className="flex items-center gap-2 pt-0.5">
                        <Icon className="size-4 text-muted-foreground" />
                      </div>
                      <div className="flex-1 min-w-0 space-y-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="text-sm font-medium">{q.title}</span>
                          <Badge variant="outline" className="text-xs">{q.points} pts</Badge>
                          <Badge
                            variant="outline"
                            className={cn(
                              "text-xs",
                              q.difficulty === "easy" && "border-success/20 text-success",
                              q.difficulty === "medium" && "border-warning/20 text-warning-foreground",
                              q.difficulty === "hard" && "border-destructive/20 text-destructive"
                            )}
                          >
                            {q.difficulty}
                          </Badge>
                          <Badge variant="secondary" className="text-xs">{questionTypeLabels[q.type]}</Badge>
                        </div>
                        <p className="text-xs text-muted-foreground line-clamp-1">{q.text}</p>
                        {q.tags && q.tags.length > 0 && (
                          <div className="flex flex-wrap gap-1 pt-1">
                            {q.tags.map((tag) => (
                              <Badge key={tag} variant="outline" className="text-[10px] px-1.5 py-0">{tag}</Badge>
                            ))}
                          </div>
                        )}
                      </div>
                      <div className="flex items-center gap-1 shrink-0">
                        <Button variant="ghost" size="sm" onClick={() => toggleExpand(q.id)}>
                          {isExpanded ? <EyeOffIcon className="size-3.5" /> : <EyeIcon className="size-3.5" />}
                        </Button>
                        {isOwner && (
                          <Button variant="ghost" size="icon" className="size-8">
                            <Trash2Icon className="size-3.5 text-destructive" />
                          </Button>
                        )}
                      </div>
                    </div>
                    {isExpanded && (
                      <>
                        <Separator />
                        <div className="p-4 space-y-3 bg-muted/30">
                          <div>
                            <Label className="text-xs text-muted-foreground">Question Text:</Label>
                            <p className="text-sm mt-1">{q.text}</p>
                          </div>
                          {q.options && (
                            <div className="space-y-1">
                              <Label className="text-xs text-muted-foreground">Options:</Label>
                              {q.options.map((opt) => (
                                <div key={opt.id} className="flex items-center gap-2 text-sm">
                                  <span className={cn("size-2 rounded-full", opt.isCorrect ? "bg-success" : "bg-muted-foreground/30")} />
                                  <span className={opt.isCorrect ? "font-medium" : ""}>{opt.text}</span>
                                  {opt.isCorrect && <Badge variant="success" className="text-[10px]">Correct</Badge>}
                                </div>
                              ))}
                            </div>
                          )}
                          {q.correctAnswer && (
                            <div>
                              <Label className="text-xs text-muted-foreground">Correct Answer:</Label>
                              <p className="text-sm font-medium mt-1">{q.correctAnswer}</p>
                            </div>
                          )}
                          {q.acceptedAnswers && (
                            <div>
                              <Label className="text-xs text-muted-foreground">Accepted Answers:</Label>
                              <ul className="text-sm mt-1 space-y-0.5 list-disc list-inside">
                                {q.acceptedAnswers.map((a, i) => <li key={i}>{a}</li>)}
                              </ul>
                            </div>
                          )}
                          {q.explanation && (
                            <div>
                              <Label className="text-xs text-muted-foreground">Explanation:</Label>
                              <p className="text-sm mt-1 text-muted-foreground">{q.explanation}</p>
                            </div>
                          )}
                        </div>
                      </>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function Label({ className, children }: { className?: string; children: React.ReactNode }) {
  return <span className={cn("block", className)}>{children}</span>;
}
