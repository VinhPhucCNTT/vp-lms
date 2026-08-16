import { api } from "@/lib/api-client";
import type { Problem, Submission, JudgeLanguage, SubmissionVerdict } from "@/types";

export interface ProblemDto {
  id: string;
  title: string;
  slug?: string;
  description: string;
  difficulty: string;
  tags: string[];
  constraints?: string[];
  examples?: { input: string; output: string; explanation?: string }[];
  testCases?: { id: string; input: string; expectedOutput: string; isHidden: boolean }[];
  starterCode?: Record<string, string>;
  timeLimit: number;
  memoryLimit: number;
  acceptedCount: number;
  submissionCount: number;
}

export interface SubmissionDto {
  id: string;
  userId: string;
  problemId?: string;
  assignmentId?: string;
  type: string;
  content: string;
  language?: string;
  verdict: string;
  score?: number;
  maxScore?: number;
  feedback?: string;
  executionTime?: number;
  memoryUsed?: number;
  submittedAt: string;
  gradedAt?: string;
  testResults?: {
    testCaseId: string;
    verdict: string;
    executionTime: number;
    memoryUsed: number;
  }[];
}

function mapProblem(dto: ProblemDto): Problem {
  return {
    id: dto.id,
    title: dto.title,
    slug: dto.slug ?? dto.title.toLowerCase().replace(/\s+/g, "-"),
    description: dto.description,
    difficulty: dto.difficulty as Problem["difficulty"],
    tags: dto.tags ?? [],
    constraints: dto.constraints ?? [],
    examples: dto.examples ?? [],
    testCases: dto.testCases ?? [],
    starterCode: (dto.starterCode ?? {}) as Record<JudgeLanguage, string>,
    timeLimit: dto.timeLimit,
    memoryLimit: dto.memoryLimit,
    acceptedCount: dto.acceptedCount,
    submissionCount: dto.submissionCount,
  };
}

function mapSubmission(dto: SubmissionDto): Submission {
  return {
    id: dto.id,
    userId: dto.userId,
    problemId: dto.problemId,
    assignmentId: dto.assignmentId,
    type: dto.type as Submission["type"],
    content: dto.content,
    language: dto.language as JudgeLanguage | undefined,
    verdict: dto.verdict as SubmissionVerdict,
    score: dto.score,
    maxScore: dto.maxScore,
    feedback: dto.feedback,
    executionTime: dto.executionTime,
    memoryUsed: dto.memoryUsed,
    submittedAt: dto.submittedAt,
    gradedAt: dto.gradedAt,
  };
}

export const judgeApi = {
  async getCourseProblems(courseId: string): Promise<Problem[]> {
    const dtos = await api.get<ProblemDto[]>(`/api/courses/${courseId}/problems`);
    return dtos.map(mapProblem);
  },

  async getProblem(problemId: string): Promise<Problem> {
    const dto = await api.get<ProblemDto>(`/api/problems/${problemId}`);
    return mapProblem(dto);
  },

  async getSubmissions(problemId: string): Promise<Submission[]> {
    const dtos = await api.get<SubmissionDto[]>(`/api/problems/${problemId}/submissions`);
    return dtos.map(mapSubmission);
  },

  async submitCode(problemId: string, language: JudgeLanguage, code: string): Promise<Submission & { testResults?: { testCaseId: string; verdict: string; executionTime: number; memoryUsed: number }[] }> {
    const dto = await api.post<SubmissionDto>(`/api/problems/${problemId}/submit`, { language, code });
    const base = mapSubmission(dto);
    return { ...base, testResults: dto.testResults };
  },

  async runCode(problemId: string, language: JudgeLanguage, code: string): Promise<{ verdict: string; testResults: { testCaseId: string; verdict: string; executionTime: number; memoryUsed: number }[] }> {
    return api.post(`/api/problems/${problemId}/run`, { language, code });
  },
};
