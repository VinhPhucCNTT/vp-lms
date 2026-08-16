import { api } from "@/lib/api-client";
import type { Assessment, Question, AssessmentAttempt, AttemptAnswer } from "@/types";

// ── DTOs ──────────────────────────────────────────────────────────────────────

export interface AssessmentDto {
  id: string;
  moduleId?: string;
  courseId?: string;
  title: string;
  description?: string;
  duration: number;
  passingScore?: number;
  maxAttempts: number;
  dueDate: string;
  availableFrom?: string;
  availableTo?: string;
  shuffleQuestions?: boolean;
  shuffleAnswers?: boolean;
  resultVisibility?: string;
  status?: string;
  totalPoints?: number;
}

export interface AssessmentInfoDto {
  description: string | null;
  timeLimitMinutes: number | null;
  maxAttempts: number;
  availableFrom: string | null;
  availableUntil: string | null;
  showResults: boolean;
}

export interface AssessmentResourceInfoDto {
  id: string;
  type: number | string;
  title: string;
  orderIndex: number;
  availableFrom: string | null;
  availableUntil: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface AssessmentDetailDto {
  resourceInfo: AssessmentResourceInfoDto;
  info: AssessmentInfoDto;
}

export interface AssessmentListDto {
  resourceInfo: AssessmentResourceInfoDto;
  info: AssessmentInfoDto;
  questionCount: number;
  attemptsUsed: number;
  bestScore: number | null;
  bestMaxScore: number;
  latestAttemptStatus: string | null;
  latestAttemptSqid: string | null;
}

export interface AssessmentAttemptStartDto {
  assessmentAttemptSqid: string;
  startedAt: string;
  submittedAt: string | null;
  totalScore: number | null;
  isPassed: boolean | null;
  attemptNumber: number;
}

export interface AssessmentAttemptQuestionDto {
  attemptQuestionSqid: string;
  questionType: string;
  text: string;
  questionData: {
    options?: { id: string; text: string }[];
  };
  orderIndex: number;
  points: number;
  isFlagged: boolean;
  answerData: Record<string, unknown> | null;
  answeredAt: string | null;
}

export interface AssessmentAttemptDetailDto extends AssessmentAttemptStartDto {
  maxScore: number;
  status: string;
  questions: AssessmentAttemptQuestionDto[];
}

export interface QuestionDto {
  id: string;
  bankId?: string;
  type: string;
  title: string;
  text: string;
  points: number;
  difficulty: string;
  options?: { id: string; text: string; isCorrect: boolean }[];
  correctAnswer?: string;
  acceptedAnswers?: string[];
  explanation?: string;
  problemId?: string;
  language?: string;
  tags?: string[];
}

export interface AssessmentWithQuestionsDto {
  assessment: AssessmentDto;
  questions: QuestionDto[];
  course?: { id: string; code: string; title: string };
}

export interface AttemptDto {
  id: string;
  assessmentId: string;
  studentId: string;
  attemptNumber: number;
  status: string;
  answers: AttemptAnswer[];
  score: number | null;
  maxScore: number;
  startedAt: string;
  submittedAt: string | null;
  timeSpent?: number;
}

export interface StudentAssessmentSummaryDto {
  assessment: AssessmentDto;
  course?: { id: string; code: string; title: string };
  status: string;
  bestScore: number | null;
  bestMaxScore: number;
  attemptsUsed: number;
  latestAttemptSqid: string | null;
  questionCount: number;
}

// ── Mappers ───────────────────────────────────────────────────────────────────

function mapAssessment(dto: AssessmentDto): Assessment {
  return {
    id: dto.id,
    moduleId: dto.moduleId ?? "",
    title: dto.title,
    description: dto.description,
    duration: dto.duration,
    passingScore: dto.passingScore ?? 0,
    maxAttempts: dto.maxAttempts,
    dueDate: dto.dueDate,
    availableFrom: dto.availableFrom,
    availableTo: dto.availableTo,
    shuffleQuestions: dto.shuffleQuestions,
    shuffleAnswers: dto.shuffleAnswers,
    resultVisibility: dto.resultVisibility as Assessment["resultVisibility"],
    status: dto.status as Assessment["status"],
    totalPoints: dto.totalPoints,
  };
}

function mapBackendAssessment(dto: AssessmentDetailDto | AssessmentListDto): AssessmentDto {
  return {
    id: dto.resourceInfo.id,
    title: dto.resourceInfo.title,
    description: dto.info.description ?? undefined,
    duration: dto.info.timeLimitMinutes ?? 0,
    maxAttempts: dto.info.maxAttempts,
    dueDate: dto.info.availableUntil ?? "",
    availableFrom: dto.info.availableFrom ?? undefined,
    availableTo: dto.info.availableUntil ?? undefined,
    status: "published",
  };
}

function getAvailabilityStatus(dto: AssessmentListDto): string {
  const now = Date.now();
  if (dto.info.availableFrom && now < new Date(dto.info.availableFrom).getTime()) return "upcoming";
  if (dto.info.availableUntil && now >= new Date(dto.info.availableUntil).getTime()) return "overdue";
  const latestStatus = dto.latestAttemptStatus?.toLowerCase();
  if (latestStatus === "inprogress") return "in-progress";
  if (latestStatus === "submitted" || latestStatus === "graded" || latestStatus === "expired") return "completed";
  return "available";
}

function mapQuestion(dto: QuestionDto): Question {
  return {
    id: dto.id,
    bankId: dto.bankId ?? "",
    type: dto.type as Question["type"],
    title: dto.title,
    text: dto.text,
    points: dto.points,
    difficulty: dto.difficulty as Question["difficulty"],
    options: dto.options,
    correctAnswer: dto.correctAnswer,
    acceptedAnswers: dto.acceptedAnswers,
    explanation: dto.explanation,
    problemId: dto.problemId,
    language: dto.language as Question["language"],
    tags: dto.tags,
  };
}

function mapAttemptQuestion(dto: AssessmentAttemptQuestionDto): Question {
  const type = dto.questionType.toLowerCase();
  const normalizedType = type === "multiplechoice"
    ? "multiple-choice"
    : type === "multipleselect"
      ? "multiple-select"
      : type === "truefalse"
        ? "true-false"
        : type === "shortanswer"
          ? "short-answer"
          : type === "coding"
            ? "programming"
            : "short-answer";

  return {
    id: dto.attemptQuestionSqid,
    bankId: "",
    type: normalizedType,
    title: `Question ${dto.orderIndex + 1}`,
    text: dto.text,
    points: dto.points,
    difficulty: "medium",
    options: dto.questionData.options,
  };
}

function mapAttemptAnswer(dto: AssessmentAttemptQuestionDto): string | string[] | undefined {
  const answer = dto.answerData;
  if (!answer) return undefined;

  if (typeof answer.value === "boolean") return answer.value ? "true" : "false";
  if (typeof answer.value === "string") return answer.value;
  if (Array.isArray(answer.selectedOptionIds)) return answer.selectedOptionIds.filter((x): x is string => typeof x === "string");
  if (typeof answer.selectedOptionId === "string") return answer.selectedOptionId;
  return undefined;
}

function mapAttempt(dto: AttemptDto): AssessmentAttempt {
  return {
    id: dto.id,
    assessmentId: dto.assessmentId,
    studentId: dto.studentId,
    attemptNumber: dto.attemptNumber,
    status: dto.status as AssessmentAttempt["status"],
    answers: dto.answers,
    score: dto.score,
    maxScore: dto.maxScore,
    startedAt: dto.startedAt,
    submittedAt: dto.submittedAt,
    timeSpent: dto.timeSpent,
  };
}

// ── API ───────────────────────────────────────────────────────────────────────

export const assessmentApi = {
  // Student: list assessments with status and attempt info
  async getStudentAssessments(): Promise<StudentAssessmentSummaryDto[]> {
    const dtos = await api.get<AssessmentListDto[]>("/api/assessments/");
    return dtos.map((dto) => ({
      assessment: mapBackendAssessment(dto),
      status: getAvailabilityStatus(dto),
      bestScore: dto.bestScore,
      bestMaxScore: dto.bestMaxScore,
      attemptsUsed: dto.attemptsUsed,
      questionCount: dto.questionCount,
      latestAttemptSqid: dto.latestAttemptSqid,
    }));
  },

  // Student: get assessment discovery/detail data
  async getAssessmentDetail(assessmentId: string): Promise<AssessmentDetailDto> {
    return api.get<AssessmentDetailDto>(`/api/assessments/${assessmentId}`);
  },

  // Student: create or resume the authenticated student's attempt
  async startAssessment(assessmentId: string): Promise<AssessmentAttemptStartDto> {
    return api.post<AssessmentAttemptStartDto>(`/api/assessments/${assessmentId}/start`);
  },

  // Student: read the persisted attempt state for the current student
  async getAttempt(assessmentId: string, attemptId: string): Promise<AssessmentAttemptDetailDto> {
    return api.get<AssessmentAttemptDetailDto>(`/api/assessments/${assessmentId}/attempt/${attemptId}`);
  },

  // Student: persist one answer without submitting the assessment
  async saveAttemptAnswer(
    assessmentId: string,
    attemptId: string,
    attemptQuestionId: string,
    answerData: Record<string, unknown>,
  ): Promise<void> {
    await api.post<void>(
      `/api/assessments/${assessmentId}/attempt/${attemptId}/question/${attemptQuestionId}/answer`,
      { answerData },
    );
  },

  // Student: finalize and grade the current attempt
  async submitAttempt(assessmentId: string, attemptId: string): Promise<AssessmentAttemptDetailDto> {
    return api.post<AssessmentAttemptDetailDto>(
      `/api/assessments/${assessmentId}/attempt/${attemptId}/submit`,
    );
  },

  // Student: get assessment with questions for taking the test
  async getAssessmentForTaking(assessmentId: string): Promise<AssessmentWithQuestionsDto> {
    return api.get<AssessmentWithQuestionsDto>(`/api/assessments/${assessmentId}/take`);
  },

  // Student: submit assessment answers
  async submitAssessment(assessmentId: string, answers: Record<string, string | string[]>): Promise<AttemptDto> {
    const answerList: AttemptAnswer[] = Object.entries(answers).map(([questionId, value]) => ({
      questionId,
      value,
    }));
    const dto = await api.post<AttemptDto>(`/api/assessments/${assessmentId}/submit`, { answers: answerList });
    return mapAttempt(dto);
  },

  // Student: get attempt results
  async getAttemptResults(assessmentId: string, attemptId: string): Promise<AttemptDto> {
    const dto = await api.get<AttemptDto>(`/api/assessments/${assessmentId}/attempts/${attemptId}`);
    return mapAttempt(dto);
  },

  // Instructor: list assessments for a course or all
  async getInstructorAssessments(): Promise<Assessment[]> {
    const dtos = await api.get<AssessmentDto[]>("/api/instructor/assessments");
    return dtos.map(mapAssessment);
  },

  // Instructor: get assessment details with questions
  async getInstructorAssessmentDetail(assessmentId: string): Promise<AssessmentWithQuestionsDto> {
    return api.get<AssessmentWithQuestionsDto>(`/api/instructor/assessments/${assessmentId}`);
  },

  // Instructor: get attempts for an assessment
  async getAttemptList(assessmentId: string): Promise<AttemptDto[]> {
    return api.get<AttemptDto[]>(`/api/instructor/assessments/${assessmentId}/attempts`);
  },

  // Instructor: get a specific attempt for review
  async getInstructorAttempt(assessmentId: string, attemptId: string): Promise<AttemptDto> {
    return api.get<AttemptDto>(`/api/instructor/assessments/${assessmentId}/attempts/${attemptId}`);
  },

  // Instructor: get assessment summary list with stats
  async getInstructorAssessmentSummaries(): Promise<InstructorAssessmentSummaryDto[]> {
    return api.get<InstructorAssessmentSummaryDto[]>("/api/instructor/assessments/summaries");
  },

  // Maps for convenience
  mapAssessment,
  mapQuestion,
  mapAttemptQuestion,
  mapAttemptAnswer,
  mapBackendAssessment,
};

export interface InstructorAssessmentSummaryDto {
  assessment: AssessmentDto;
  questionCount: number;
  totalPoints: number;
  attemptCount: number;
  needsGrading: number;
}
