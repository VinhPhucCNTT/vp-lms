import { api } from "@/lib/api-client";
import type { Course } from "@/types";

export interface InstructorStatsDto {
  totalStudents: number;
  publishedCourses: number;
  totalCourses: number;
  pendingSubmissions: number;
  pendingAssignments: number;
  pendingAssessments: number;
  gradedThisWeek: number;
}

export interface PendingSubmissionDto {
  id: string;
  studentName: string;
  assignmentTitle: string;
  courseCode: string;
  submittedAt: string;
  urgent: boolean;
}

export interface AnnouncementDto {
  id: string;
  title: string;
  content: string;
  courseId: string;
  courseCode?: string;
  isPinned: boolean;
  createdAt: string;
}

export interface GradebookStudentDto {
  studentId: string;
  studentName: string;
  email: string;
  assignments: { title: string; score: number; maxScore: number }[];
  assessments: { title: string; score: number; maxScore: number }[];
  finalGrade: string;
  percentage: number;
}

export interface SubmissionDetailDto {
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
  student: { id: string; name: string; email: string };
  course: { code: string; title: string };
  assignmentTitle: string;
}

export const instructorApi = {
  async getStats(): Promise<InstructorStatsDto> {
    return api.get<InstructorStatsDto>("/api/instructor/stats");
  },

  async getPendingSubmissions(): Promise<PendingSubmissionDto[]> {
    return api.get<PendingSubmissionDto[]>("/api/instructor/submissions/pending");
  },

  async getRecentCourses(): Promise<Course[]> {
    const dtos = await api.get<Course[]>("/api/courses/instructor");
    return dtos;
  },

  async getAnnouncements(): Promise<AnnouncementDto[]> {
    return api.get<AnnouncementDto[]>("/api/instructor/announcements");
  },

  async createAnnouncement(data: { title: string; content: string; courseId: string; isPinned: boolean }): Promise<AnnouncementDto> {
    return api.post<AnnouncementDto>("/api/instructor/announcements", data);
  },

  async deleteAnnouncement(id: string): Promise<void> {
    await api.delete(`/api/instructor/announcements/${id}`);
  },

  async getGradebook(courseId: string): Promise<GradebookStudentDto[]> {
    return api.get<GradebookStudentDto[]>(`/api/instructor/gradebook/${courseId}`);
  },

  async getAllSubmissions(): Promise<SubmissionDetailDto[]> {
    return api.get<SubmissionDetailDto[]>("/api/instructor/submissions");
  },

  async gradeSubmission(submissionId: string, grade: number, feedback: string): Promise<void> {
    await api.post(`/api/instructor/submissions/${submissionId}/grade`, { grade, feedback });
  },
};
