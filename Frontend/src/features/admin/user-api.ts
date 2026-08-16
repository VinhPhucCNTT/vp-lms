import { api } from "@/lib/api-client";
import type { User, UserRole, UserStatus } from "@/types";

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  status: UserStatus;
  createdAt: string;
  updatedAt: string;
  studentId?: string;
  department?: string;
  enrolledCourses?: string[];
  gpa?: number;
}

function mapUser(dto: UserDto): User {
  const base: User = {
    id: dto.id,
    email: dto.email,
    firstName: dto.firstName,
    lastName: dto.lastName,
    role: dto.role,
    status: dto.status,
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt,
  };
  if (dto.role === "student" && dto.studentId) {
    return { ...base, studentId: dto.studentId, enrolledCourses: dto.enrolledCourses ?? [], gpa: dto.gpa ?? 0 } as User;
  }
  if (dto.role === "instructor") {
    return { ...base, department: dto.department ?? "" } as User;
  }
  if (dto.role === "admin") {
    return { ...base, permissions: [] } as User;
  }
  return base;
}

export const userApi = {
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
