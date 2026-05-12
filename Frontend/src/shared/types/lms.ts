/** Mirrors Backend.Models.Enums.ActivityType */
export const ActivityType = {
  Lesson: 0,
  Assessment: 1,
  Assignment: 2,
} as const;
export type ActivityType =
  (typeof ActivityType)[keyof typeof ActivityType];

/** Mirrors Backend.Models.Enums.QuestionType */
export const QuestionType = {
  SingleChoice: 0,
  MultipleChoice: 1,
} as const;
export type QuestionType =
  (typeof QuestionType)[keyof typeof QuestionType];

/** Mirrors Backend.Models.Enums.AssessmentType (quiz vs exam metadata) */
export const AssessmentType = {
  Quiz: 0,
  Exam: 1,
} as const;
export type AssessmentType =
  (typeof AssessmentType)[keyof typeof AssessmentType];

export interface ViewCourse {
  id: string;
  title: string;
  description: string | null;
  thumbnailUrl: string | null;
  isPublished: boolean;
  instructorId: string;
  instructorUserName: string | null;
  instructorFullName: string;
  moduleCount: number;
  enrollmentCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCoursePayload {
  title: string;
  description?: string | null;
  thumpnailUrl?: string | null;
  isPublished: boolean;
}

export interface UpdateCoursePayload {
  title: string;
  description?: string | null;
  thumpnailUrl?: string | null;
  isPublished: boolean;
}

export interface ViewActivity {
  id: string;
  title: string;
  type: ActivityType;
  orderIndex: number;
  isPublished: boolean;
  availableFrom: string | null;
  availableUntil: string | null;
  resourceId: string | null;
}

export interface ViewModule {
  id: string;
  title: string;
  orderIndex: number;
  activities: ViewActivity[] | null;
}

export interface CourseContent {
  id: string;
  title: string;
  modules: ViewModule[];
}

export interface ViewLesson {
  id: string;
  contentHtml: string;
  videoUrl: string | null;
  attachmentUrl: string | null;
}

export interface ViewAssignment {
  id: string;
  instructions: string;
  dueDate: string | null;
  allowLateSubmission: boolean;
  maxPoints: number;
}

export interface SubmitAssignmentPayload {
  submissionText: string;
  fileUrl?: string | null;
}

export interface ViewAssessment {
  id: string;
  type: AssessmentType;
  timeLimitMinutes: number;
  maxAttempts: number;
  passingScore: number;
  shuffleQuestions: boolean;
}

export interface ViewQuestionOption {
  id: string;
  optionText: string;
  isCorrect: boolean;
}

export interface ViewQuestion {
  id: string;
  questionText: string;
  type: QuestionType;
  points: number;
  orderIndex: number;
  options: ViewQuestionOption[];
}

export interface SubmitAnswerPayload {
  questionId: string;
  answerText: string[];
}

export interface SubmitAssessmentPayload {
  answers: SubmitAnswerPayload[];
}

export interface AssessmentResult {
  /** Backend returns score (decimal) — JSON number */
  score: number;
  passed: boolean;
}

export interface CreateModulePayload {
  title: string;
  orderIndex: number;
}

export interface CreateActivityPayload {
  title: string;
  type: ActivityType;
  orderIndex: number;
  isPublished: boolean;
  availableFrom?: string | null;
  availableUntil?: string | null;
}

export interface UpdateActivityPayload {
  title: string;
  type: ActivityType;
  orderIndex: number;
  isPublished: boolean;
  availableFrom?: string | null;
  availableUntil?: string | null;
}

export interface CreateLessonPayload {
  contentHtml: string;
  videoUrl?: string | null;
  attachmentUrl?: string | null;
}

export interface UpdateLessonPayload {
  contentHtml: string;
  videoUrl?: string | null;
  attachmentUrl?: string | null;
}

export interface CreateAssignmentPayload {
  instructions: string;
  dueDate?: string | null;
  allowLateSubmission: boolean;
  maxPoints: number;
}

export interface UpdateAssignmentPayload {
  instructions: string;
  dueDate?: string | null;
  allowLateSubmission: boolean;
  maxPoints: number;
}

export interface CreateAssessmentPayload {
  type: AssessmentType;
  timeLimitMinutes: number;
  maxAttempts: number;
  password?: string | null;
  passingScore: number;
  shuffleQuestions: boolean;
}

export interface UpdateAssessmentPayload {
  type: AssessmentType;
  timeLimitMinutes: number;
  maxAttempts: number;
  password?: string | null;
  passingScore: number;
  shuffleQuestions: boolean;
}

export interface CreateAssessmentQuestionPayload {
  questionText: string;
  type: QuestionType;
  points: number;
  orderIndex: number;
  options: { optionText: string; isCorrect: boolean }[];
}

export interface UpdateAssessmentQuestionPayload {
  questionText: string;
  type: QuestionType;
  points: number;
  orderIndex: number;
  options: { optionText: string; isCorrect: boolean }[];
}
