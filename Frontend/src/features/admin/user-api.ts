import { api } from "@/lib/api-client";
import type { User, UserRole } from "@/types";

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  avatarFileId: string;
  createdAt: string;
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

export const userApi = {
    async updateSelf(data: Partial<UserDto>): Promise<User> {
        const dto = await api.patch<UserDto>("/api/user", data)
        return mapUser(dto)
    },
}
