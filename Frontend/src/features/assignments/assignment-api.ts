import { api } from "@/lib/api-client";
import type { Assignment, StudentActivity } from "@/types";

export interface AssignmentDto {
  id: string;
  moduleId?: string;
  courseId?: string;
  title: string;
  description: string;
  dueDate: string;
  maxScore: number;
  weight?: number;
  status?: string;
  score?: number;
  submittedAt?: string;
  course?: { id: string; code: string; title: string };
}

export interface AssignmentSummaryDto {
  assignment: AssignmentDto;
  course?: { id: string; code: string; title: string };
  status: string;
  score?: number;
  submittedAt?: string;
}

export interface StudentActivityDto {
  id: string;
  title: string;
  type: string;
  course: { id: string; code: string; title: string };
  dueDate: string;
  status: string;
  refId: string;
  moduleId: string;
}

function mapAssignment(dto: AssignmentDto): Assignment {
  return {
    id: dto.id,
    moduleId: dto.moduleId ?? "",
    title: dto.title,
    description: dto.description,
    dueDate: dto.dueDate,
    maxScore: dto.maxScore,
    weight: dto.weight ?? 0,
  };
}

function mapActivity(dto: StudentActivityDto): StudentActivity {
  return {
    id: dto.id,
    title: dto.title,
    type: dto.type as StudentActivity["type"],
    course: dto.course,
    dueDate: dto.dueDate,
    status: dto.status as StudentActivity["status"],
    refId: dto.refId,
    moduleId: dto.moduleId,
  };
}

export const assignmentApi = {
  async getStudentAssignments(): Promise<AssignmentSummaryDto[]> {
    return api.get<AssignmentSummaryDto[]>("/api/assignments");
  },

  async getAssignment(assignmentId: string): Promise<Assignment> {
    const dto = await api.get<AssignmentDto>(`/api/assignments/${assignmentId}`);
    return mapAssignment(dto);
  },

  async submitAssignment(assignmentId: string, content: string, type: "text" | "file" = "text"): Promise<void> {
    await api.post(`/api/assignments/${assignmentId}/submit`, { content, type });
  },

  async getStudentActivity(): Promise<StudentActivity[]> {
    const dtos = await api.get<StudentActivityDto[]>("/api/activity");
    return dtos.map(mapActivity);
  },

  mapAssignment,
};
