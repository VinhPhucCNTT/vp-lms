import { api } from "@/lib/api-client";
import type { Problem, Submission, JudgeLanguage, SubmissionVerdict } from "@/types";

interface CodingProblemDto {
  resourceInfo: { id: string; title: string };
  info: {
    problemStatementMarkdown: string;
    constraintsMarkdown?: string;
    language: string;
    timeLimitMs: number;
    memoryLimitMb: number;
    difficulty: string | number;
  };
}

interface CodingSubmissionDto {
  id: string;
  status: string;
  executionTimeMs?: number;
  memoryUsedKb?: number;
  passedTestCases?: number;
  totalTestCases?: number;
}

function verdict(value: string): SubmissionVerdict {
  return value
    .replace(/([a-z])([A-Z])/g, "$1-$2")
    .toLowerCase() as SubmissionVerdict;
}

function mapCodingProblem(dto: CodingProblemDto): Problem {
  const difficulty = typeof dto.info.difficulty === "number"
    ? (["unspecified", "easy", "medium", "hard"][dto.info.difficulty] ?? "unspecified")
    : dto.info.difficulty.toLowerCase();

  return {
    id: dto.resourceInfo.id,
    title: dto.resourceInfo.title,
    slug: dto.resourceInfo.title.toLowerCase().replace(/\s+/g, "-"),
    description: dto.info.problemStatementMarkdown,
    difficulty: difficulty as Problem["difficulty"],
    tags: [],
    constraints: dto.info.constraintsMarkdown ? [dto.info.constraintsMarkdown] : [],
    examples: [],
    testCases: [],
    starterCode: {} as Record<JudgeLanguage, string>,
    timeLimit: dto.info.timeLimitMs,
    memoryLimit: dto.info.memoryLimitMb,
    acceptedCount: 0,
    submissionCount: 0,
  };
}

function mapCodingSubmission(dto: CodingSubmissionDto): Submission {
  return {
    id: dto.id,
    userId: "",
    problemId: undefined,
    type: "code",
    content: "",
    verdict: verdict(dto.status),
    executionTime: dto.executionTimeMs,
    memoryUsed: dto.memoryUsedKb ? dto.memoryUsedKb / 1024 : undefined,
    submittedAt: "",
  };
}

export const judgeApi = {
  async getCourseProblems(_courseId: string): Promise<Problem[]> {
    // Course-level problem listing is not part of the current backend API.
    return [];
  },

  async getProblem(problemId: string): Promise<Problem> {
    const dto = await api.get<CodingProblemDto>(`/api/coding-problems/${problemId}`);
    return mapCodingProblem(dto);
  },

  async getSubmissions(_problemId: string): Promise<Submission[]> {
    // Submission history is not exposed by the current API; the single
    // submission endpoint is still used by submitCode while it is pending.
    return [];
  },

  async submitCode(problemId: string, language: JudgeLanguage, code: string): Promise<Submission & { testResults?: { testCaseId: string; verdict: string; executionTime: number; memoryUsed: number }[] }> {
    const created = await api.post<CodingSubmissionDto>(`/api/coding-submissions/${problemId}`, {
      sourceCode: code,
      language: language === "javascript" ? "TypeScript" : language === "cpp" ? "Cpp" : language[0].toUpperCase() + language.slice(1),
    });

    let result = created;
    for (let attempt = 0; attempt < 60 && result.status.toLowerCase() === "pending"; attempt++) {
      await new Promise((resolve) => setTimeout(resolve, 500));
      result = await api.get<CodingSubmissionDto>(`/api/coding-submissions/${created.id}`);
    }

    return { ...mapCodingSubmission(result), id: result.id };
  },

  async runCode(problemId: string, language: JudgeLanguage, code: string): Promise<{ verdict: string; testResults: { testCaseId: string; verdict: string; executionTime: number; memoryUsed: number }[] }> {
    const result = await this.submitCode(problemId, language, code);
    return { verdict: result.verdict, testResults: result.testResults ?? [] };
  },
};
