import { api, getBaseUrl } from "@/lib/api-client";
import type { Assignment, StudentActivity } from "@/types";

export interface AssignmentResourceInfoDto {
  id: string;
  type: number | string;
  title: string;
  orderIndex: number;
  availableFrom: string | null;
  availableUntil: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface AssignmentInfoDto {
  instructionsMD: string;
  submissionType: number | string;
  allowedExtensions: string[] | null;
  maxFileSizeKb: number;
  maxFileCount: number | null;
  minTextLength: number | null;
  maxTextLength: number | null;
  openDate: string | null;
  closeDate: string | null;
  gradingSchemaJson: string | null;
}

export interface AssignmentResponseDto {
  resourceInfo: AssignmentResourceInfoDto;
  info: AssignmentInfoDto;
}

export interface CourseSummaryDto {
  id: string;
  code: string;
  title: string;
  description: string | null;
}

export interface StudentAssignmentSummaryDto {
  assignment: AssignmentResponseDto;
  course: CourseSummaryDto;
  status: "pending" | "submitted" | "graded" | "overdue" | string;
  submittedAt: string | null;
  score: number | null;
  feedbackText: string | null;
  submittedFileCount: number;
}

export interface FileResponseDto {
  id: string;
  userId: string;
  originalFileName: string;
  contentType: string;
  sizeInBytes: number;
  sha256Hash: string;
}

interface AssignmentFileResponseDto {
  resourceId: string;
  fileInfo: FileResponseDto;
}

export interface SubmissionDetailDto {
  assignmentId: string;
  userId: string;
  submissionText: string | null;
  submittedOn: string | null;
  status: "not-submitted" | "submitted" | "graded" | string;
  score: number | null;
  feedbackText: string | null;
  files: FileResponseDto[];
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

function mapAssignment(dto: AssignmentResponseDto): Assignment {
  return {
    id: dto.resourceInfo.id,
    moduleId: "",
    title: dto.resourceInfo.title,
    description: dto.info.instructionsMD,
    dueDate: dto.info.closeDate ?? "",
    maxScore: 0,
    weight: 0,
  };
}

export const assignmentApi = {
  async getStudentAssignments(): Promise<StudentAssignmentSummaryDto[]> {
    return api.get<StudentAssignmentSummaryDto[]>("/api/assignment/");
  },

  async getAssignment(assignmentId: string): Promise<AssignmentResponseDto> {
    return api.get<AssignmentResponseDto>(`/api/assignment/${assignmentId}`);
  },

  async getOwnSubmission(assignmentId: string): Promise<SubmissionDetailDto> {
    return api.get<SubmissionDetailDto>(`/api/assignment/${assignmentId}/submission/student-self`);
  },

  async uploadAssignmentFile(assignmentId: string, file: File): Promise<FileResponseDto> {
    const form = new FormData();
    form.append("file", file);
    const response = await api.post<AssignmentFileResponseDto>(`/api/assignment/${assignmentId}/upload`, form);
    return response.fileInfo;
  },

  async submitAssignment(assignmentId: string, submissionText: string | null): Promise<SubmissionDetailDto> {
    return api.post<SubmissionDetailDto>(`/api/assignment/${assignmentId}/submit`, { submissionText });
  },

  async getStudentActivity(): Promise<StudentActivity[]> {
    const dtos = await api.get<StudentActivityDto[]>("/api/activity");
    return dtos.map((dto) => ({
      id: dto.id,
      title: dto.title,
      type: dto.type as StudentActivity["type"],
      course: dto.course,
      dueDate: dto.dueDate,
      status: dto.status as StudentActivity["status"],
      refId: dto.refId,
      moduleId: dto.moduleId,
    }));
  },

  getFileUrl(fileId: string): string {
    return `${getBaseUrl()}/api/file/${fileId}`;
  },

  mapAssignment,
};
