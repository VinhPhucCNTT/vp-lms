import { z } from "zod";
import { api } from "@/lib/api-client";
import { imageFileSchema, PageRequest } from "@/shared/schemas";
import { NonNegativeInteger, PaginatedResponse } from "@/shared/types";
import { ActivityResponse } from "@/features/activity/activity-api";
import { Course } from "../types/course";

// Backend DTOs — these match the ASP.NET Core API response shapes.
// IDs are Sqid-encoded strings and should be treated as opaque.

//// COURSE

// REQUESTS

export const courseQuerySchema = z.object({
    pageNumber: z.number().int().min(1),
    pageSize: z.number().int().min(1),
    title: z.string().nullable(),
    description: z.string().nullable(),
    enrollmentOpen: z.boolean().nullable()
});
export type CourseQueryRequest = z.infer<typeof courseQuerySchema>;

export const courseCreateSchema = z.object({
    title: z.string(),
    description: z.string().nullable(),
    enrollmentOpen: z.boolean(),
    backgroundFile: imageFileSchema.nullable()
});
export type CourseCreateRequest = z.infer<typeof courseCreateSchema>;

export const coursePatchSchema = courseCreateSchema.partial();
export type CoursePatchRequest = z.infer<typeof coursePatchSchema>;

// REQUEST MAPPER

export function toCourseCreateFormData(
    data: CourseCreateRequest
): FormData {
    const formData = new FormData();

    formData.append("title", data.title);
    formData.append("description", data.description ?? "");
    formData.append("enrollmentOpen", String(data.enrollmentOpen));

    if (data.backgroundFile) {
        formData.append("backgroundFile", data.backgroundFile);
    }

    return formData;
}

// RESPONSES

export interface CourseResponse {
    id: string,
    creatorId: string,
    title: string,
    description: string,
    backgroundFileId?: string
}

export interface CourseSetResponse {
    id: string,
    creatorId: string,
    title: string,
    description?: string,
    isPublished: boolean,
    enrollmentOpen: boolean,
    backgroundFileId?: string
}

export interface CourseRecentResponse {
    courseId: string,
    courseName: string,
    backgroundFileId: string,
}

export interface CourseDetailResponse {
    id: string,
    instructorId: string,
    instructorFirstName: string,
    instructorLastName: string,
    backgroundFileId?: string,
    title: string,
    description?: string,
    enrollmentCount: number
}

export interface CourseProgress {
    completed: NonNegativeInteger<number>,
    total: NonNegativeInteger<number>
}

export interface StudentCourseResponse {
    info: CourseDetailResponse,
    progress: CourseProgress
}

export interface InstructorCourseResponse {
    info: CourseDetailResponse,
    isPublished: boolean
}

export interface ExploreCourseResponse {
    featured: CourseDetailResponse[],
    byDepartment: Map<string, CourseDetailResponse[]>,
    recent: CourseDetailResponse[]
}

//// MODULE

// REQUESTS

export const moduleSetSchema = z.object({
    title: z.string(),
    description: z.string().nullable(),
    orderIndex: z.number().int(),
    isPublished: z.boolean()
});
export type ModuleSetRequest = z.infer<typeof moduleSetSchema>;

// RESPONSES

export interface ModuleResponse {
    id: string,
    title: string,
    description?: string,
    orderIndex: NonNegativeInteger<number>
}

export interface ModuleSetResponse {
    id: string,
    title: string,
    description?: string,
    orderIndex: NonNegativeInteger<number>
    isPublished?: boolean
}

// //// VIEW MODELS (Defined in ../types)
//
// function mapCourseDetail(courseDto: CourseDetailResponse, moduleDto: ModuleResponse, activitiesDto: ActivityResponse): Course {
//   return {
//     id: dto.id,
//     title: dto.title,
//     description: dto.description ?? "",
//     instructorId: dto.creatorId,
//     enrolledCount: dto.enrollmentCount,
//     startDate: "",
//     endDate: "",
//     createdAt: "",
//     department: undefined,
//     credits: undefined,
//     level: undefined,
//     semester: undefined,
//     tags: undefined,
//     featured: undefined,
//   };
// }
//
// API

export const courseApi = {
    // Student: list enrolled courses
    async getEnrolledCourses(): Promise<PaginatedResponse<StudentCourseResponse>> {
        const response = await api.get<PaginatedResponse<StudentCourseResponse>>("/api/course/student");
        return response;
    },

    // Student: list all published courses (explore)
    async getPublishedCourses(): Promise<ExploreCourseResponse> {
        const response = await api.get<ExploreCourseResponse>("/api/course/explore");
        return response;
    },

    // Instructor: list courses created by the current instructor
    async getInstructorCourses(): Promise<PaginatedResponse<InstructorCourseResponse>> {
        const dtos = await api.get<PaginatedResponse<InstructorCourseResponse>>("/api/course/instructor");
        return dtos;
    },

    // TODO: Implement (admin dto and endpoint)
    // Admin: list all courses
    // async getAllCourses(): Promise<Course[]> {
    //   const response = await api.get<PaginatedCourseDto>("/api/course");
    //   return response.data.map(mapCourse);
    // },

    // Get a single course with modules, activities, lessons
    // TODO: This might not be needed
    async getCourseDetail(courseId: string, page: PageRequest): Promise<Course> {
        const pageParams = new URLSearchParams({
            pageNumber: page.pageNumber.toString(),
            pageSize: page.pageSize.toString()
        }).toString();

        const course = await api.get<CourseDetailResponse>(`/api/course/${courseId}?` + pageParams);
        const modules = await api.get<PaginatedResponse<ModuleResponse>>(`/api/course/${courseId}/modules?` + pageParams);

        let activities = new Map<string, PaginatedResponse<ActivityResponse>>;
        for (let module of modules.data) {
            const response = await api.get<PaginatedResponse<ActivityResponse>>(`/api/module/${module.id}/activities?` + pageParams);
            activities.set(module.id, response);
        }

        return new Course(course, modules, activities);
    },

    // TODO: Move activities related apis to activity feature
    // // Student: list published resources in a module
    // async getModuleActivities(moduleId: string, page: PageRequest): Promise<PaginatedResponse<ActivityResponse>> {
    //   const pageParams = new URLSearchParams({
    //       pageNumber: page.pageNumber.toString(),
    //       pageSize: page.pageSize.toString()
    //   }).toString();
    //
    //   return api.get<PaginatedResponse<ActivityResponse>>(`/api/module/${moduleId}/activities?` + pageParams);
    // },
    //
    // // Student: get the content for a lesson resource
    // async getLesson(resourceId: string): Promise<LessonDto> {
    //   return api.get<LessonDto>(`/api/lesson/${resourceId}`);
    // },
    //
    // async getResourceProgress(resourceId: string): Promise<ResourceProgressDto> {
    //   return api.get<ResourceProgressDto>(`/api/resource/${resourceId}/progress`);
    // },
    //
    // async completeResource(resourceId: string): Promise<ResourceProgressDto> {
    //   return api.post<ResourceProgressDto>(`/api/resource/${resourceId}/complete`);
    // },

    // Enroll in a course
    async enroll(courseId: string): Promise<void> {
        await api.post(`/api/enrollment/enroll/${courseId}`);
    },

    // Unenroll from a course
    async unenroll(courseId: string): Promise<void> {
        await api.post(`/api/enrollment/unenroll/${courseId}`);
    },

    async createCourse(data: CourseCreateRequest): Promise<CourseSetResponse> {
        const response = await api.post<CourseSetResponse>('/api/course/', toCourseCreateFormData(data));
        return response;
    },

    // Update a course
    async updateCourse(courseId: string, data: CoursePatchRequest): Promise<CourseSetResponse> {
        const response = await api.put<CourseSetResponse>(`/api/course/${courseId}`, data);
        return response;
    },

    // Publish / archive
    async publish(courseId: string): Promise<void> {
        await api.post(`/api/course/${courseId}/publish`);
    },

    async unpublish(courseId: string): Promise<void> {
        await api.post(`/api/course/${courseId}/unpublish`);
    },
};
