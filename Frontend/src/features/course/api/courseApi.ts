import { api } from "@/shared/services/api";
import type {
    CourseContent,
    CreateCoursePayload,
    UpdateCoursePayload,
    ViewCourse,
} from "@/shared/types/lms";

export const getCourses = async (): Promise<ViewCourse[]> => {
    const { data } = await api.get<ViewCourse[]>("/courses/");
    return data;
};

export const getCourse = async (id: string): Promise<ViewCourse> => {
    const { data } = await api.get<ViewCourse>(`/courses/${id}`);
    return data;
};

export const getCourseContent = async (
    courseId: string,
): Promise<CourseContent> => {
    const { data } = await api.get<CourseContent>(
        `/courses/${courseId}/content`,
    );
    return data;
};

export const createCourse = async (
    payload: CreateCoursePayload,
): Promise<string> => {
    const { data } = await api.put<string>("/courses/", payload);
    return data;
};

export const updateCourse = async (
    id: string,
    payload: UpdateCoursePayload,
): Promise<void> => {
    await api.post(`/courses/${id}`, payload);
};

export const deleteCourse = async (id: string): Promise<void> => {
    await api.delete(`/courses/${id}`);
};
