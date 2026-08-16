import * as React from "react";
import { SearchIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/shared/components/page-header";
import { CourseCard } from "@/shared/components/course-card";
import { useApi } from "@/lib/use-api";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { courseApi } from "@/features/courses/course-api";
import type { Course } from "@/types";

export function StudentCourses() {
  const { data: enrolledCourses, loading, error, reload } = useApi<Course[]>(() => courseApi.getEnrolledCourses());
  const [search, setSearch] = React.useState("");

  const filteredCourses = React.useMemo(() => {
    if (!enrolledCourses) return [];
    return enrolledCourses.filter((course) =>
      course.title.toLowerCase().includes(search.toLowerCase()) || course.code.toLowerCase().includes(search.toLowerCase())
    );
  }, [enrolledCourses, search]);

  if (loading) return <LoadingState label="Loading your courses..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <PageHeader title="My Courses" description="Browse and manage your enrolled courses" breadcrumbs={[{ label: "Dashboard", href: "/student" }, { label: "My Courses" }]} />

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search courses..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
      </div>

      <Tabs defaultValue="all">
        <TabsList>
          <TabsTrigger value="all">All ({filteredCourses.length})</TabsTrigger>
          <TabsTrigger value="in-progress">In Progress ({filteredCourses.length})</TabsTrigger>
          <TabsTrigger value="completed">Completed (0)</TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="mt-6">
          {filteredCourses.length === 0 ? (
            <EmptyState message="You are not enrolled in any courses yet." />
          ) : (
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {filteredCourses.map((course) => (
                <CourseCard key={course.id} course={course} progress={0} showProgress />
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="in-progress" className="mt-6">
          {filteredCourses.length === 0 ? (
            <EmptyState message="No courses in progress." />
          ) : (
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {filteredCourses.map((course) => (
                <CourseCard key={course.id} course={course} progress={0} showProgress />
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="completed" className="mt-6">
          <EmptyState message="No completed courses yet." />
        </TabsContent>
      </Tabs>
    </div>
  );
}
