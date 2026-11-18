import { z } from "zod";
import { api } from "@/lib/api-client";
import { PaginatedResponse } from "@/shared/types";

//// REQUESTS

export const submissionRequestSchema = z.object({
    submissionText: z.string().nullable()
});
export type SubmissionRequest = z.infer<typeof submissionRequestSchema>;

export const assignmentGradeRequestSchema = z.object({
    score: z.number(),
    feedbackText: z.string().nullable()
});
export type AssignmentGradeRequest = z.infer<typeof assignmentGradeRequestSchema>;

//// RESPONSES

// NOTE: MOVE AFTER
export interface FileResponse {
    id: string,
    userId: string,
    originalFileName: string,
    contentType: string,
    sizeInBytes: number,
    sha256Hash: string
}

export interface SubmissionResponse {
    assignmentId: string,
    submissionId: string,
    userId: string
}

export interface SubmissionDetailResponse {
    assignmentId: string,
    userId: string,
    submissionText?: string,
    submittedOn?: string,
    status: string,
    score?: number,
    feedbackText?: string,
    files: FileResponse[]
}

export interface AssignmentGradeResponse {
    id: string,
    submissionId: string,
    graderId: string,
    score: number,
    feedbackText?: string
}

export interface InstructorStatsResponse {
    totalStudents: number;
    coursesPublished: number;
    coursesTotal: number;
    pendingAssignments: number;
    pendingSubmissions: number;
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
    async getStats(): Promise<InstructorStatsResponse> {
        return api.get<InstructorStatsResponse>("/api/dashboard/instructor");
    },

    async getPendingSubmissions(): Promise<SubmissionResponse[]> {
        return api.get<SubmissionResponse[]>("/api/assignment-submission/pending/instructor");
    },

    // TODO: Implement
    async getRecentCourses(): Promise<CourseResponse[]> {
      const dtos = await api.get<CourseReponse[]>("/api/courses/recent/instructor");
      return dtos;
    },

    // TODO
    async getAnnouncements(): Promise<AnnouncementDto[]> {
      return api.get<AnnouncementDto[]>("/api/instructor/announcements");
    },

    async createannouncement(data: { title: string; content: string; courseid: string; ispinned: boolean }): promise<announcementdto> {
      return api.post<AnnouncementDto>("/api/instructor/announcements", data);
    },

    async deleteAnnouncement(id: string): Promise<void> {
      await api.delete(`/api/instructor/announcements/${id}`);
    },

    async getGradebook(courseId: string): Promise<GradebookStudentDto[]> {
      return api.get<GradebookStudentDto[]>(`/api/instructor/gradebook/${courseId}`);
    },

    async getAllSubmissions(): Promise<PaginatedResponse<SubmissionResponse>> {
        return api.get<PaginatedResponse<SubmissionResponse>>("/api/assignment-submission/instructor");
    },

    async gradeSubmission(submissionId: string, grade: number, feedback: string): Promise<void> {
        await api.post(`/api/instructor/submissions/${submissionId}/grade`, { grade, feedback });
    },

    async addGrade(submissionId: string, request: AssignmentGradeRequest): Promise<AssignmentGradeResponse> {
        return api.post<AssignmentGradeResponse>(`/api/assignment-submission/${submissionId}/grade`, request);
    },

    async updateGrade(submissionId: string, request: AssignmentGradeRequest): Promise<AssignmentGradeResponse> {
        return api.put<AssignmentGradeResponse>(`/api/assignment-submission/${submissionId}/grade`, request);
    }
};
