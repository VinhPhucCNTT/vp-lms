import { api } from "@/lib/api-client";
import { User } from "@/types";
import { UserDto } from "./user-api";

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

function mapUser(dto: UserDto): User {
  const base: User = {
    id: dto.id,
    email: dto.email,
    firstName: dto.firstName,
    lastName: dto.lastName,
    role: dto.role,
    avatarFileId: dto.avatarFileId,
    createdAt: dto.createdAt,
  };
  // if (dto.role === "student") {
  //   return { ...base, studentId: dto.Id, enrolledCourses: dto.enrolledCourses ?? [], gpa: dto.gpa ?? 0 } as User;
  // }
  // if (dto.role === "instructor") {
  //   return { ...base, department: dto.department ?? "" } as User;
  // }
  // if (dto.role === "admin") {
  //   return { ...base, permissions: [] } as User;
  // }
  return base;
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

export const adminUserApi = {
  async getAllUsers(): Promise<User[]> {
    const dtos = await api.get<UserDto[]>("/api/admin/users");
    return dtos.map(mapUser);
  },

  async createUser(data: { firstName: string; lastName: string; email: string; role: string }): Promise<User> {
    const dto = await api.post<UserDto>("/api/admin/users", data);
    return mapUser(dto);
  },

  async updateUser(userId: string, data: Partial<UserDto>): Promise<User> {
    const dto = await api.put<UserDto>(`/api/admin/users/${userId}`, data);
    return mapUser(dto);
  },

  async deleteUser(userId: string): Promise<void> {
    await api.delete(`/api/admin/users/${userId}`);
  },

  async suspendUser(userId: string): Promise<void> {
    await api.post(`/api/admin/users/${userId}/suspend`);
  },

  async activateUser(userId: string): Promise<void> {
    await api.post(`/api/admin/users/${userId}/activate`);
  },
};
