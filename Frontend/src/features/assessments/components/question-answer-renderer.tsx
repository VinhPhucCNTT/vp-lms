import * as React from "react";
import Editor from "@monaco-editor/react";
import {
  CircleDotIcon,
  ListChecksIcon,
  ToggleLeftIcon,
  TextIcon,
  AlignLeftIcon,
  CodeIcon,
  type LucideIcon,
} from "lucide-react";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Checkbox } from "@/components/ui/checkbox";
import { Switch } from "@/components/ui/switch";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { Question, QuestionType, JudgeLanguage } from "@/types";
import { questionTypeLabels } from "@/shared/data/question-bank";
import { problems } from "@/shared/data/problems";

export const questionTypeIconMap: Record<QuestionType, LucideIcon> = {
  "multiple-choice": CircleDotIcon,
  "multiple-select": ListChecksIcon,
  "true-false": ToggleLeftIcon,
  "short-answer": TextIcon,
  "essay": AlignLeftIcon,
  "programming": CodeIcon,
};

const languageOptions: { value: JudgeLanguage; label: string }[] = [
  { value: "python", label: "Python 3" },
  { value: "javascript", label: "JavaScript" },
  { value: "cpp", label: "C++ 17" },
  { value: "java", label: "Java 17" },
];

interface QuestionAnswerRendererProps {
  question: Question;
  value: string | string[];
  onChange: (value: string | string[]) => void;
  disabled?: boolean;
  showCorrect?: boolean;
}

export function QuestionAnswerRenderer({
  question,
  value,
  onChange,
  disabled,
  showCorrect,
}: QuestionAnswerRendererProps) {
  const Icon = questionTypeIconMap[question.type];

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <Icon className="size-4 text-muted-foreground" />
        <Badge variant="outline" className="text-xs">
          {questionTypeLabels[question.type]}
        </Badge>
        <Badge variant="outline" className="text-xs">
          {question.points} pts
        </Badge>
      </div>

      <div className="prose prose-sm dark:prose-invert max-w-none">
        <p className="text-base leading-relaxed">{question.text}</p>
      </div>

      <div className="pt-2">
        <AnswerInput
          question={question}
          value={value}
          onChange={onChange}
          disabled={disabled}
          showCorrect={showCorrect}
        />
      </div>
    </div>
  );
}

function AnswerInput({
  question,
  value,
  onChange,
  disabled,
  showCorrect,
}: {
  question: Question;
  value: string | string[];
  onChange: (value: string | string[]) => void;
  disabled?: boolean;
  showCorrect?: boolean;
}) {
  switch (question.type) {
    case "multiple-choice":
      return (
        <RadioGroup
          value={value as string}
          onValueChange={onChange}
          disabled={disabled}
          className="space-y-2"
        >
          {question.options?.map((opt) => (
            <div
              key={opt.id}
              className={cn(
                "flex items-center gap-3 rounded-lg border p-3 transition-colors",
                value === opt.id && "border-primary bg-primary/5",
                showCorrect && opt.isCorrect && "border-success bg-success/5",
                showCorrect && value === opt.id && !opt.isCorrect && "border-destructive bg-destructive/5"
              )}
            >
              <RadioGroupItem value={opt.id} id={opt.id} />
              <Label htmlFor={opt.id} className="flex-1 cursor-pointer text-sm font-normal">
                {opt.text}
              </Label>
              {showCorrect && opt.isCorrect && (
                <Badge variant="success" className="text-xs">Correct</Badge>
              )}
            </div>
          ))}
        </RadioGroup>
      );

    case "multiple-select":
      return (
        <div className="space-y-2">
          {question.options?.map((opt) => {
            const selected = Array.isArray(value) && value.includes(opt.id);
            return (
              <div
                key={opt.id}
                className={cn(
                  "flex items-center gap-3 rounded-lg border p-3 transition-colors",
                  selected && "border-primary bg-primary/5",
                  showCorrect && opt.isCorrect && "border-success bg-success/5",
                  showCorrect && selected && !opt.isCorrect && "border-destructive bg-destructive/5"
                )}
              >
                <Checkbox
                  checked={selected}
                  onCheckedChange={(checked) => {
                    const current = Array.isArray(value) ? value : [];
                    if (checked) onChange([...current, opt.id]);
                    else onChange(current.filter((v) => v !== opt.id));
                  }}
                  disabled={disabled}
                  id={opt.id}
                />
                <Label htmlFor={opt.id} className="flex-1 cursor-pointer text-sm font-normal">
                  {opt.text}
                </Label>
                {showCorrect && opt.isCorrect && (
                  <Badge variant="success" className="text-xs">Correct</Badge>
                )}
              </div>
            );
          })}
        </div>
      );

    case "true-false":
      return (
        <RadioGroup
          value={value as string}
          onValueChange={onChange}
          disabled={disabled}
          className="flex gap-4"
        >
          <div
            className={cn(
              "flex items-center gap-2 rounded-lg border px-4 py-2 transition-colors",
              value === "true" && "border-primary bg-primary/5"
            )}
          >
            <RadioGroupItem value="true" id={`${question.id}-true`} />
            <Label htmlFor={`${question.id}-true`} className="cursor-pointer">True</Label>
          </div>
          <div
            className={cn(
              "flex items-center gap-2 rounded-lg border px-4 py-2 transition-colors",
              value === "false" && "border-primary bg-primary/5"
            )}
          >
            <RadioGroupItem value="false" id={`${question.id}-false`} />
            <Label htmlFor={`${question.id}-false`} className="cursor-pointer">False</Label>
          </div>
        </RadioGroup>
      );

    case "short-answer":
      return (
        <Input
          value={value as string}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          placeholder="Enter your answer..."
          className="max-w-xl"
        />
      );

    case "essay":
      return (
        <Textarea
          value={value as string}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          placeholder="Write your response here..."
          className="min-h-[200px]"
        />
      );

    case "programming":
      return <ProgrammingAnswer question={question} value={value as string} onChange={onChange} disabled={disabled} />;
  }
}

function ProgrammingAnswer({
  question,
  value,
  onChange,
  disabled,
}: {
  question: Question;
  value: string;
  onChange: (value: string | string[]) => void;
  disabled?: boolean;
}) {
  const problem = question.problemId ? problems.find((p) => p.id === question.problemId) : null;
  const [language, setLanguage] = React.useState<JudgeLanguage>(question.language ?? "python");

  const starterCode = React.useMemo(() => {
    if (problem?.starterCode[language]) return problem.starterCode[language];
    return "";
  }, [problem, language]);

  React.useEffect(() => {
    if (!value && starterCode) onChange(starterCode);
  }, [starterCode]);

  return (
    <div className="space-y-3">
      {problem && (
        <div className="rounded-lg border bg-muted/30 p-3 text-sm text-muted-foreground">
          <span className="font-medium text-foreground">{problem.title}</span>
          {" — "}
          Submit your solution below. Your code will be evaluated against test cases.
        </div>
      )}
      <div className="flex items-center justify-between rounded-t-lg border border-b-0 bg-muted/50 px-3 py-2">
        <span className="text-xs font-medium text-muted-foreground">Code Editor</span>
        <Select value={language} onValueChange={(v) => setLanguage(v as JudgeLanguage)}>
          <SelectTrigger className="h-7 w-36 text-xs">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {languageOptions.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="h-[300px] rounded-b-lg border overflow-hidden">
        <Editor
          height="100%"
          language={language === "cpp" ? "cpp" : language}
          value={value || starterCode}
          onChange={(v) => onChange(v ?? "")}
          theme="vs-dark"
          options={{
            minimap: { enabled: false },
            fontSize: 13,
            padding: { top: 12 },
            scrollBeyondLastLine: false,
            readOnly: disabled,
          }}
        />
      </div>
    </div>
  );
}
