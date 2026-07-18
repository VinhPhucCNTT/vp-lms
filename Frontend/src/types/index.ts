export type UserRole = "student" | "instructor" | "admin";
export type UserStatus = "active" | "suspended" | "pending";
export type CourseLevel = "beginner" | "intermediate" | "advanced";
export type ActivityStatus = "pending" | "completed" | "overdue";

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  status: UserStatus;
  createdAt: string;
  updatedAt: string;
}

export interface Student extends User {
  role: "student";
  studentId: string;
  enrolledCourses: string[];
  gpa: number;
}

export interface Instructor extends User {
  role: "instructor";
  department: string;
  officeLocation?: string;
}

export interface Admin extends User {
  role: "admin";
  permissions: string[];
}

export interface Course {
  id: string;
  code: string;
  title: string;
  description: string;
  instructorId: string;
  status: "draft" | "published" | "archived";
  enrolledCount: number;
  startDate: string;
  endDate: string;
  createdAt: string;
  department?: string;
  credits?: number;
  level?: CourseLevel;
  semester?: string;
  tags?: string[];
  featured?: boolean;
}

export interface Module {
  id: string;
  courseId: string;
  title: string;
  order: number;
  activityIds: string[];
}

export type ActivityType = "lesson" | "assignment" | "assessment" | "coding-problem";

export interface CourseActivity {
  id: string;
  moduleId: string;
  courseId: string;
  type: ActivityType;
  title: string;
  order: number;
  refId: string;
}

export interface Lesson {
  id: string;
  moduleId: string;
  title: string;
  type: "video" | "reading" | "interactive";
  content: string;
  duration: number;
  order: number;
}

export interface Assignment {
  id: string;
  moduleId: string;
  title: string;
  description: string;
  dueDate: string;
  maxScore: number;
  weight: number;
}

export interface Assessment {
  id: string;
  moduleId: string;
  title: string;
  description: string;
  duration: number;
  passingScore: number;
  maxAttempts: number;
  dueDate: string;
}

export type ProblemDifficulty = "easy" | "medium" | "hard";
export type JudgeLanguage = "cpp" | "java" | "python" | "javascript";

export interface Problem {
  id: string;
  title: string;
  slug: string;
  description: string;
  difficulty: ProblemDifficulty;
  tags: string[];
  constraints: string[];
  examples: { input: string; output: string; explanation?: string }[];
  testCases: { id: string; input: string; expectedOutput: string; isHidden: boolean }[];
  starterCode: Record<JudgeLanguage, string>;
  timeLimit: number;
  memoryLimit: number;
  acceptedCount: number;
  submissionCount: number;
}

export type SubmissionVerdict =
  | "accepted"
  | "wrong-answer"
  | "time-limit-exceeded"
  | "memory-limit-exceeded"
  | "runtime-error"
  | "compilation-error"
  | "pending";

export interface Submission {
  id: string;
  userId: string;
  problemId?: string;
  assignmentId?: string;
  type: "code" | "file" | "text";
  content: string;
  language?: JudgeLanguage;
  verdict: SubmissionVerdict;
  score?: number;
  maxScore?: number;
  feedback?: string;
  executionTime?: number;
  memoryUsed?: number;
  submittedAt: string;
  gradedAt?: string;
}

export interface Announcement {
  id: string;
  courseId: string;
  authorId: string;
  title: string;
  content: string;
  isPinned: boolean;
  createdAt: string;
}

export interface StudentActivity {
  id: string;
  title: string;
  type: ActivityType;
  course: { id: string; code: string; title: string };
  dueDate: string;
  status: ActivityStatus;
  refId: string;
  moduleId: string;
}
