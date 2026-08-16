import { api } from "@/lib/api-client";
import type { Course } from "@/types";

// Backend DTOs — these match the ASP.NET Core API response shapes.
// IDs are Sqid-encoded strings and should be treated as opaque.

export interface CourseDto {
  id: string;
  creatorId: string;
  creatorUsername: string;
  creatorFullname: string;
  code: string;
  title: string;
  description: string | null;
  enrollmentCount: number;
}

export interface CourseModuleDto {
  id: string;
  description: string | null;
  title: string;
  orderIndex: number;
}

export interface CourseDetailDto {
  course: CourseDto;
  modules: CourseModuleDto[];
}

export type ResourceType = "Lesson" | "Assignment" | "Assessment" | "Problem" | number;

export interface CourseResourceDto {
  id: string;
  type: ResourceType;
  title: string;
  orderIndex: number;
}

export interface LessonDto {
  resourceInfo: CourseResourceDto & {
    availableFrom: string | null;
    availableUntil: string | null;
    createdAt: string;
    updatedAt: string;
  };
  info: {
    contentMarkdown: string;
  };
}

export interface ResourceProgressDto {
  isCompleted: boolean;
  completedAt: string | null;
  lastAccessedAt: string | null;
}

interface CourseStudentDto {
  info: CourseDto;
  progress: { completed: number; total: number };
}

interface PaginatedCourseDto {
  data: CourseDto[];
}

function mapCourse(dto: CourseDto): Course {
  return {
    id: dto.id,
    code: dto.code,
    title: dto.title,
    description: dto.description ?? "",
    instructorId: dto.creatorId,
    status: "published",
    enrolledCount: dto.enrollmentCount,
    startDate: "",
    endDate: "",
    createdAt: "",
    department: undefined,
    credits: undefined,
    level: undefined,
    semester: undefined,
    tags: undefined,
    featured: undefined,
  };
}

export const courseApi = {
  // Student: list enrolled courses
  async getEnrolledCourses(): Promise<Course[]> {
    const dtos = await api.get<CourseStudentDto[]>("/api/course/student");
    return dtos.map((dto) => mapCourse(dto.info));
  },

  // Student: list all published courses (explore)
  async getPublishedCourses(): Promise<Course[]> {
    const response = await api.get<PaginatedCourseDto>("/api/course");
    return response.data.map(mapCourse);
  },

  // Instructor: list courses created by the current instructor
  async getInstructorCourses(): Promise<Course[]> {
    const dtos = await api.get<CourseDto[]>("/api/course/instructor");
    return dtos.map(mapCourse);
  },

  // Admin: list all courses
  async getAllCourses(): Promise<Course[]> {
    const response = await api.get<PaginatedCourseDto>("/api/course");
    return response.data.map(mapCourse);
  },

  // Get a single course with modules, activities, lessons
  async getCourseDetail(courseId: string): Promise<CourseDetailDto> {
    const course = await api.get<CourseDto>(`/api/course/${courseId}`);
    const modules = await api.get<CourseModuleDto[]>(`/api/course/${courseId}/modules`);
    return { course, modules };
  },

  // Student: list published resources in a module
  async getModuleResources(moduleId: string): Promise<CourseResourceDto[]> {
    return api.get<CourseResourceDto[]>(`/api/resource/module/${moduleId}`);
  },

  // Student: get the content for a lesson resource
  async getLesson(resourceId: string): Promise<LessonDto> {
    return api.get<LessonDto>(`/api/lesson/${resourceId}`);
  },

  async getResourceProgress(resourceId: string): Promise<ResourceProgressDto> {
    return api.get<ResourceProgressDto>(`/api/resource/${resourceId}/progress`);
  },

  async completeResource(resourceId: string): Promise<ResourceProgressDto> {
    return api.post<ResourceProgressDto>(`/api/resource/${resourceId}/complete`);
  },

  // Enroll in a course
  async enroll(courseId: string): Promise<void> {
    await api.post(`/api/enrollment/enroll/${courseId}`);
  },

  // Unenroll from a course
  async unenroll(courseId: string): Promise<void> {
    await api.post(`/api/enrollment/unenroll/${courseId}`);
  },

  // Create a course (instructor)
  async createCourse(data: Partial<Course>): Promise<Course> {
    const dto = await api.post<CourseDto>("/api/course", data);
    return mapCourse(dto);
  },

  // Update a course
  async updateCourse(courseId: string, data: Partial<Course>): Promise<Course> {
    const dto = await api.put<CourseDto>(`/api/course/${courseId}`, data);
    return mapCourse(dto);
  },

  // Publish / archive
  async updateStatus(courseId: string, status: string): Promise<void> {
    await api.post(`/api/course/${courseId}/${status === "published" ? "publish" : "unpublish"}`);
  },
};
