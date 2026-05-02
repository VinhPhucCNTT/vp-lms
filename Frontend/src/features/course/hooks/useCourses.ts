import { useQuery } from "@tanstack/react-query";
import { getCourses } from "@/features/courses/api/courseApi";

export const useCourses = () =>
  useQuery({
    queryKey: ["courses"],
    queryFn: getCourses,
  });
