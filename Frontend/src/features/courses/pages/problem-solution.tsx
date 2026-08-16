import * as React from "react";
import { useParams, Link } from "react-router-dom";
import { PlayIcon, SendIcon, RotateCcwIcon, ClockIcon, MemoryStickIcon, CheckCircleIcon, XCircleIcon, AlertCircleIcon, ChevronLeftIcon } from "lucide-react";
import Editor from "@monaco-editor/react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { judgeApi } from "@/features/courses/judge-api";
import { cn } from "@/lib/utils";
import type { JudgeLanguage, Submission, SubmissionVerdict } from "@/types";

const verdictColors: Record<SubmissionVerdict, string> = {
  accepted: "bg-success text-success-foreground",
  "wrong-answer": "bg-destructive text-destructive-foreground",
  "time-limit-exceeded": "bg-warning text-warning-foreground",
  "memory-limit-exceeded": "bg-warning text-warning-foreground",
  "runtime-error": "bg-destructive text-destructive-foreground",
  "compilation-error": "bg-destructive text-destructive-foreground",
  pending: "bg-muted text-muted-foreground",
};

const verdictIcons: Record<SubmissionVerdict, React.ReactNode> = {
  accepted: <CheckCircleIcon className="size-4" />,
  "wrong-answer": <XCircleIcon className="size-4" />,
  "time-limit-exceeded": <ClockIcon className="size-4" />,
  "memory-limit-exceeded": <MemoryStickIcon className="size-4" />,
  "runtime-error": <AlertCircleIcon className="size-4" />,
  "compilation-error": <AlertCircleIcon className="size-4" />,
  pending: <div className="size-4 rounded-full border-2 border-muted-foreground border-t-transparent animate-spin" />,
};

const languageOptions: { value: JudgeLanguage; label: string }[] = [
  { value: "python", label: "Python 3" },
  { value: "javascript", label: "JavaScript" },
  { value: "cpp", label: "C++ 17" },
  { value: "java", label: "Java 17" },
];

interface RunResult {
  verdict: SubmissionVerdict;
  testResults: { testCaseId: string; verdict: string; executionTime: number; memoryUsed: number }[];
}

export function ProblemSolution() {
  const { problemId, courseId } = useParams<{ problemId: string; courseId: string }>();
  const { data: problem, loading, error, reload } = useApi(
    () => judgeApi.getProblem(problemId!),
    [problemId],
  );
  const { data: problemSubmissions } = useApi<Submission[]>(
    () => judgeApi.getSubmissions(problemId!),
    [problemId],
  );

  const [language, setLanguage] = React.useState<JudgeLanguage>("python");
  const [code, setCode] = React.useState("");
  const [isRunning, setIsRunning] = React.useState(false);
  const [activeTab, setActiveTab] = React.useState("description");
  const [results, setResults] = React.useState<RunResult | null>(null);
  const [runError, setRunError] = React.useState<string | null>(null);

  React.useEffect(() => {
    if (problem) {
      const starter = problem.starterCode[language] ?? "";
      setCode(starter);
    }
  }, [language, problem]);

  const handleRun = async () => {
    if (!problem) return;
    setIsRunning(true);
    setResults(null);
    setRunError(null);
    try {
      const res = await judgeApi.runCode(problem.id, language, code);
      setResults({
        verdict: res.verdict as SubmissionVerdict,
        testResults: res.testResults ?? [],
      });
    } catch (err: unknown) {
      setRunError(err instanceof Error ? err.message : "Failed to run code.");
    } finally {
      setIsRunning(false);
    }
  };

  const handleSubmit = async () => {
    if (!problem) return;
    setIsRunning(true);
    setResults(null);
    setRunError(null);
    try {
      const res = await judgeApi.submitCode(problem.id, language, code);
      setResults({
        verdict: res.verdict,
        testResults: res.testResults ?? [],
      });
    } catch (err: unknown) {
      setRunError(err instanceof Error ? err.message : "Failed to submit code.");
    } finally {
      setIsRunning(false);
    }
  };

  const handleReset = () => {
    if (problem) setCode(problem.starterCode[language] ?? "");
    setResults(null);
    setRunError(null);
  };

  if (loading) return <LoadingState label="Loading problem..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;
  if (!problem) return <ErrorState message="Problem not found." onRetry={reload} />;

  return (
    <div className="h-[calc(100vh-4rem)] flex flex-col">
      <div className="border-b bg-card px-4 py-2">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Link to={`/student/courses/${courseId}/problems`} className="text-muted-foreground hover:text-foreground"><ChevronLeftIcon className="size-5" /></Link>
            <h1 className="text-lg font-bold">{problem.title}</h1>
            <Badge className={cn(problem.difficulty === "easy" && "bg-success text-success-foreground", problem.difficulty === "medium" && "bg-warning text-warning-foreground", problem.difficulty === "hard" && "bg-destructive text-destructive-foreground")}>{problem.difficulty}</Badge>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" onClick={handleReset} disabled={isRunning}><RotateCcwIcon className="size-4 mr-2" />Reset</Button>
            <Button variant="secondary" onClick={handleRun} disabled={isRunning}><PlayIcon className="size-4 mr-2" />Run</Button>
            <Button onClick={handleSubmit} disabled={isRunning}><SendIcon className="size-4 mr-2" />Submit</Button>
          </div>
        </div>
      </div>

      <div className="flex-1 flex overflow-hidden">
        <div className="w-1/2 border-r flex flex-col">
          <Tabs value={activeTab} onValueChange={setActiveTab} className="flex-1 flex flex-col">
            <div className="border-b px-4">
              <TabsList className="h-10">
                <TabsTrigger value="description" className="text-sm">Description</TabsTrigger>
                <TabsTrigger value="submissions" className="text-sm">Submissions ({problemSubmissions?.length ?? 0})</TabsTrigger>
              </TabsList>
            </div>

            <ScrollArea className="flex-1">
              <TabsContent value="description" className="p-4 mt-0 space-y-4">
                <div>
                  <h2 className="text-lg font-semibold mb-2">Problem Statement</h2>
                  <p className="whitespace-pre-wrap text-sm">{problem.description}</p>
                </div>
                <Separator />
                {problem.constraints.length > 0 && (
                  <div>
                    <h3 className="font-semibold mb-2">Constraints</h3>
                    <ul className="list-disc list-inside space-y-1 text-sm text-muted-foreground">
                      {problem.constraints.map((constraint, i) => (<li key={i}>{constraint}</li>))}
                    </ul>
                  </div>
                )}
                {problem.examples.length > 0 && (
                  <>
                    <Separator />
                    <div>
                      <h3 className="font-semibold mb-2">Examples</h3>
                      {problem.examples.map((example, i) => (
                        <div key={i} className="bg-muted rounded-lg p-4 mb-3">
                          <div className="mb-2"><span className="text-xs font-medium text-muted-foreground">Input:</span><pre className="text-sm mt-1 font-mono">{example.input}</pre></div>
                          <div className="mb-2"><span className="text-xs font-medium text-muted-foreground">Output:</span><pre className="text-sm mt-1 font-mono">{example.output}</pre></div>
                          {example.explanation && <div><span className="text-xs font-medium text-muted-foreground">Explanation:</span><p className="text-sm mt-1">{example.explanation}</p></div>}
                        </div>
                      ))}
                    </div>
                  </>
                )}
              </TabsContent>

              <TabsContent value="submissions" className="p-4 mt-0">
                <div className="space-y-3">
                  {(!problemSubmissions || problemSubmissions.length === 0) && (
                    <p className="text-sm text-muted-foreground text-center py-8">No submissions yet.</p>
                  )}
                  {problemSubmissions?.map((submission) => (
                    <Card key={submission.id} size="sm">
                      <CardContent className="flex items-center justify-between p-3">
                        <div className="flex items-center gap-3">
                          {verdictIcons[submission.verdict]}
                          <div>
                            <p className="text-sm font-medium capitalize">{submission.verdict.replace(/-/g, " ")}</p>
                            <p className="text-xs text-muted-foreground">{submission.language} / {submission.submittedAt}</p>
                          </div>
                        </div>
                        <div className="text-right text-sm">
                          <p>{submission.executionTime}ms</p>
                          <p className="text-muted-foreground">{submission.memoryUsed}MB</p>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </TabsContent>
            </ScrollArea>
          </Tabs>
        </div>

        <div className="w-1/2 flex flex-col">
          <div className="border-b px-4 py-2 flex items-center justify-between">
            <Select value={language} onValueChange={(v) => setLanguage(v as JudgeLanguage)}>
              <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
              <SelectContent>
                {languageOptions.map((opt) => (<SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex-1">
            <Editor height="100%" language={language === "cpp" ? "cpp" : language} value={code} onChange={(value) => setCode(value ?? "")} theme="vs-dark" options={{ minimap: { enabled: false }, fontSize: 14, padding: { top: 16 }, scrollBeyondLastLine: false }} />
          </div>

          {(results || runError) && (
            <div className="border-t max-h-60 overflow-auto bg-muted/50">
              <div className="p-4">
                {runError && <p className="text-sm text-destructive mb-3">{runError}</p>}
                {results && (
                  <>
                    <div className="flex items-center justify-between mb-3">
                      <div className="flex items-center gap-2">
                        {verdictIcons[results.verdict]}
                        <span className="font-medium capitalize">{results.verdict.replace(/-/g, " ")}</span>
                      </div>
                      <Badge className={verdictColors[results.verdict]}>{results.testResults.filter((r) => r.verdict === "accepted").length}/{results.testResults.length} passed</Badge>
                    </div>
                    <div className="space-y-2">
                      {results.testResults.map((result, i) => (
                        <div key={result.testCaseId} className="flex items-center justify-between p-2 rounded bg-background">
                          <div className="flex items-center gap-2">
                            <span className="text-sm">Test Case {i + 1}</span>
                            <Badge variant="outline" className={cn(result.verdict === "accepted" && "border-success text-success", result.verdict !== "accepted" && "border-destructive text-destructive")}>{result.verdict}</Badge>
                          </div>
                          <div className="text-sm text-muted-foreground">{result.executionTime}ms / {result.memoryUsed}MB</div>
                        </div>
                      ))}
                    </div>
                  </>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
