import { ActivityResponse } from "@/features/activity/activity-api";
import { CourseDetailResponse, ModuleResponse } from "../api/course-api";
import { PaginatedResponse } from "@/shared/types";

export class Course {
    id: string;
    title: string;
    description?: string;
    backgroundFileId?: string;
    instructorId: string;
    instructorFirstName: string;
    instructorLastName: string;
    enrollmentCount: number;

    modules: PaginatedResponse<ModuleResponse>;
    acitivities: Map<string, PaginatedResponse<ActivityResponse>>;

    constructor(
        courseDto: CourseDetailResponse,
        moduleDtos: PaginatedResponse<ModuleResponse>,
        activityDtos: Map<string, PaginatedResponse<ActivityResponse>>) {
            this.id = courseDto.id;
            this.title = courseDto.title;
            this.description = courseDto.description;
            this.backgroundFileId = courseDto.backgroundFileId;
            this.instructorId = courseDto.instructorId;
            this.instructorFirstName = courseDto.instructorFirstName;
            this.instructorLastName = courseDto.instructorLastName;
            this.enrollmentCount = courseDto.enrollmentCount;

            this.modules = moduleDtos;
            this.acitivities = activityDtos;
    }
}
