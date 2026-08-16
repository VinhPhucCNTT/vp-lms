import { api } from "@/lib/api-client";

export interface AdminStatsDto {
  totalUsers: number;
  studentCount: number;
  instructorCount: number;
  adminCount: number;
  publishedCourses: number;
  totalCourses: number;
  totalEnrollments: number;
  activeUsers: number;
  averageGrade: number;
  completionRate: number;
}

export interface CourseStatDto {
  id: string;
  code: string;
  title: string;
  enrolled: number;
  avgGrade: number;
  completion: number;
}

export interface TopPerformerDto {
  id: string;
  firstName: string;
  lastName: string;
  studentId: string;
  gpa: number;
}

export interface AuditLogDto {
  id: string;
  user: string;
  action: string;
  time: string;
}

export const adminApi = {
  async getStats(): Promise<AdminStatsDto> {
    return api.get<AdminStatsDto>("/api/admin/stats");
  },

  async getCourseStats(): Promise<CourseStatDto[]> {
    return api.get<CourseStatDto[]>("/api/admin/reports/courses");
  },

  async getTopPerformers(): Promise<TopPerformerDto[]> {
    return api.get<TopPerformerDto[]>("/api/admin/reports/top-performers");
  },

  async getRecentAudit(): Promise<AuditLogDto[]> {
    return api.get<AuditLogDto[]>("/api/admin/audit?limit=4");
  },
};
