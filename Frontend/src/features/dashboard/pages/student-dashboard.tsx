import * as React from "react";
import { BookOpenIcon, FileTextIcon, ClipboardCheckIcon, ClockIcon, TrendingUpIcon, CodeIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Button } from "@/components/ui/button";
import { StatCard } from "@/shared/components/stat-card";
import { PageHeader } from "@/shared/components/page-header";
import { CourseCard } from "@/shared/components/course-card";
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { courseApi } from "@/features/courses/course-api";
import { useAuth } from "@/features/auth/auth-context";
import type { Course } from "@/types";

export function StudentDashboard() {
  const { user } = useAuth();
  const { data: enrolledCourses, loading, error, reload } = useApi<Course[]>(() => courseApi.getEnrolledCourses());

  const courseCount = enrolledCourses?.length ?? 0;

  return (
    <div className="space-y-6">
      <PageHeader title={`Welcome back, ${user?.firstName ?? ""}!`} description="Here's an overview of your academic progress" />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Active Courses" value={courseCount} icon={<BookOpenIcon className="size-5" />} variant="info" />
        <StatCard title="Pending Assignments" value={0} icon={<FileTextIcon className="size-5" />} variant="warning" />
        <StatCard title="Pending Assessments" value={0} icon={<ClipboardCheckIcon className="size-5" />} variant="warning" />
        <StatCard title="Coding Problems" value={0} icon={<CodeIcon className="size-5" />} variant="success" />
      </div>

      {loading ? (
        <LoadingState label="Loading your courses..." />
      ) : error ? (
        <ErrorState message={error} onRetry={reload} />
      ) : (
        <div className="grid gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-6">
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle>My Courses</CardTitle>
                    <CardDescription>Your enrolled courses</CardDescription>
                  </div>
                  <Link to="/student/courses"><Button variant="outline" size="sm">View All</Button></Link>
                </div>
              </CardHeader>
              <CardContent>
                {!enrolledCourses || enrolledCourses.length === 0 ? (
                  <div className="text-center py-12 text-muted-foreground">
                    You are not enrolled in any courses yet.{" "}
                    <Link to="/student/explore" className="text-primary underline">Explore courses</Link>
                  </div>
                ) : (
                  <div className="grid gap-4 sm:grid-cols-2">
                    {enrolledCourses.slice(0, 4).map((course) => (
                      <CourseCard key={course.id} course={course} progress={0} showProgress variant="compact" />
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </div>

          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2"><TrendingUpIcon className="size-5" />Progress Overview</CardTitle>
                <CardDescription>Your academic performance</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-sm text-muted-foreground">Progress data will appear once you complete activities.</p>
              </CardContent>
            </Card>
          </div>
        </div>
      )}
    </div>
  );
}
