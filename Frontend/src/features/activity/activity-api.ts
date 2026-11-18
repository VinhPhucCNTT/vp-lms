import { NonNegativeInteger } from "@/shared/types";
import { z } from "zod";

export enum ActivityType {
    Lesson = 0,
    Assignment,
    Assessment,
    Problem
};

// REQUESTS

const activityRequestInfoSchema = z.object({
    title: z.string(),
    orderIndex: z.number().int().nonnegative(),
    isPublished: z.boolean()
});

// RESPONSES

export interface ActivityResponse {
    id: string,
    type: ActivityType,
    title: string,
    orderIndex: NonNegativeInteger<number>
};

// TODO: Implement
export interface StudentActivityResponse {
    id: string,
    type: ActivityType,
    title: string,
    orderIndex: NonNegativeInteger<number>,
    statusIsCompleted: boolean
};

// TODO: Implement
export interface InstructorActivityResponse {
    id: string,
    type: ActivityType,
    title: string,
    orderIndex: NonNegativeInteger<number>,
    isPublished: boolean
}

export interface ActivityDetailResponse {
    id: string,
    type: ActivityType,
    title: string,
    orderIndex: NonNegativeInteger<number>,
    availableFrom?: string,
    availableUntil?: string,
    createdAt: string,
    updatedAt: string
}

// export interface CourseResourceDto {
//   id: string;
//   type: ResourceType;
//   title: string;
//   orderIndex: number;
// }
//
// export interface LessonDto {
//   resourceInfo: CourseResourceDto & {
//     availableFrom: string | null;
//     availableUntil: string | null;
//     createdAt: string;
//     updatedAt: string;
//   };
//   info: {
//     contentMarkdown: string;
//   };
// }
//
// export interface ResourceProgressDto {
//   isCompleted: boolean;
//   completedAt: string | null;
//   lastAccessedAt: string | null;
// }
//
// interface CourseStudentDto {
//   info: CourseDto;
//   progress: { completed: number; total: number };
// }
//
// interface PaginatedCourseDto {
//   data: CourseDto[];
// }
//
