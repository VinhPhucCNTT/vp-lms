import * as React from "react";
import { BookOpenIcon, SearchIcon, UsersIcon } from "lucide-react";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/api-states";
import { ApiError } from "@/lib/api-client";
import { useApi } from "@/lib/use-api";
import {
  courseApi,
  type CourseDetailResponse,
  type ExploreCourseResponse,
} from "@/features/courses/api/course-api";

interface ExplorePageData {
  courses: ExploreCourseResponse;
  enrolledCourseIds: string[];
}

interface ExploreCourseCardProps {
  course: CourseDetailResponse;
  enrolled: boolean;
  enrolling: boolean;
  onEnroll: (courseId: string) => void;
}

function ExploreCourseCard({ course, enrolled, enrolling, onEnroll }: ExploreCourseCardProps) {
  const instructorName = `${course.instructorFirstName} ${course.instructorLastName}`.trim();

  return (
    <Card className="group hover:shadow-md transition-shadow overflow-hidden">
      <div className="h-24 bg-gradient-to-br from-primary/20 to-accent/20 relative flex items-center justify-center">
        <BookOpenIcon className="size-8 text-primary/40" />
      </div>
      <CardHeader className="pb-2">
        <CardTitle className="text-base line-clamp-1">{course.title}</CardTitle>
        <CardDescription className="line-clamp-2">
          {course.description || "No description provided."}
        </CardDescription>
      </CardHeader>
      <CardContent className="pb-2">
        {instructorName && (
          <p className="text-sm text-muted-foreground">Instructor: {instructorName}</p>
        )}
      </CardContent>
      <CardFooter className="pt-2 border-t">
        <div className="flex items-center justify-between gap-3 w-full">
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <UsersIcon className="size-3" />
            <span>{course.enrollmentCount} students</span>
          </div>
          {enrolled ? (
            <Button size="sm" variant="secondary" disabled>Enrolled</Button>
          ) : course.enrollmentOpen ? (
            <Button size="sm" onClick={() => onEnroll(course.id)} disabled={enrolling}>
              {enrolling ? "Enrolling..." : "Enroll"}
            </Button>
          ) : (
            <Button size="sm" variant="outline" disabled>Closed</Button>
          )}
        </div>
      </CardFooter>
    </Card>
  );
}

export function StudentExplore() {
  const [search, setSearch] = React.useState("");
  const [enrolledCourseIds, setEnrolledCourseIds] = React.useState<Set<string>>(new Set());
  const [enrollingCourseIds, setEnrollingCourseIds] = React.useState<Set<string>>(new Set());
  const [enrollmentError, setEnrollmentError] = React.useState<string | null>(null);
  const inFlightEnrollments = React.useRef(new Set<string>());

  const { data, loading, error, reload } = useApi<ExplorePageData>(async () => {
    const [courses, enrollments] = await Promise.all([
      courseApi.getPublishedCourses(),
      courseApi.getCurrentEnrollments(),
    ]);
    return {
      courses,
      enrolledCourseIds: enrollments.map((enrollment) => enrollment.courseId),
    };
  });

  React.useEffect(() => {
    if (data) setEnrolledCourseIds(new Set(data.enrolledCourseIds));
  }, [data]);

  const matchesSearch = React.useCallback((course: CourseDetailResponse) => {
    const query = search.trim().toLowerCase();
    if (!query) return true;
    return [
      course.title,
      course.description ?? "",
      course.instructorFirstName,
      course.instructorLastName,
    ].some((value) => value.toLowerCase().includes(query));
  }, [search]);

  const filteredFeatured = data?.courses.featured.filter(matchesSearch) ?? [];
  const filteredRecent = data?.courses.recent.filter(matchesSearch) ?? [];
  const filteredDepartments = Object.entries(data?.courses.byDepartment ?? {})
    .map(([department, courses]) => [department, courses.filter(matchesSearch)] as const)
    .filter(([, courses]) => courses.length > 0);

  const visibleCourseIds = new Set([
    ...filteredFeatured.map((course) => course.id),
    ...filteredRecent.map((course) => course.id),
    ...filteredDepartments.flatMap(([, courses]) => courses.map((course) => course.id)),
  ]);

  async function handleEnroll(courseId: string) {
    if (inFlightEnrollments.current.has(courseId) || enrolledCourseIds.has(courseId)) return;

    inFlightEnrollments.current.add(courseId);
    setEnrollingCourseIds((current) => new Set(current).add(courseId));
    setEnrollmentError(null);

    try {
      await courseApi.enroll(courseId);
      setEnrolledCourseIds((current) => new Set(current).add(courseId));
      reload();
    } catch (err: unknown) {
      setEnrollmentError(err instanceof ApiError || err instanceof Error
        ? err.message
        : "Unable to enroll in this course.");
    } finally {
      inFlightEnrollments.current.delete(courseId);
      setEnrollingCourseIds((current) => {
        const next = new Set(current);
        next.delete(courseId);
        return next;
      });
    }
  }

  function renderCourses(courses: CourseDetailResponse[]) {
    return courses.map((course) => (
      <ExploreCourseCard
        key={course.id}
        course={course}
        enrolled={enrolledCourseIds.has(course.id)}
        enrolling={enrollingCourseIds.has(course.id)}
        onEnroll={handleEnroll}
      />
    ));
  }

  if (loading) return <LoadingState label="Loading courses..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-8">
      <PageHeader
        title="Explore Courses"
        description="Discover new courses to enhance your learning journey"
        breadcrumbs={[{ label: "Dashboard", href: "/student" }, { label: "Explore" }]}
      />

      <div className="relative max-w-xl">
        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
        <Input
          placeholder="Search courses or instructors..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          className="pl-9"
        />
      </div>

      {enrollmentError && (
        <p role="alert" className="text-sm text-destructive">{enrollmentError}</p>
      )}

      {visibleCourseIds.size === 0 ? (
        <EmptyState message={search ? "No courses match your search." : "No published courses are available."} />
      ) : (
        <>
          {filteredFeatured.length > 0 && (
            <div>
              <h2 className="text-xl font-semibold mb-4">Featured / Recommended</h2>
              <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                {renderCourses(filteredFeatured)}
              </div>
            </div>
          )}

          {filteredFeatured.length > 0 && filteredDepartments.length > 0 && <Separator />}

          {filteredDepartments.map(([department, courses]) => (
            <div key={department}>
              <h2 className="text-xl font-semibold mb-4">Browse by Department &mdash; {department}</h2>
              <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                {renderCourses(courses)}
              </div>
            </div>
          ))}

          {filteredRecent.length > 0 && (
            <>
              <Separator />
              <div>
                <h2 className="text-xl font-semibold mb-4">Recently Updated</h2>
                <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                  {renderCourses(filteredRecent)}
                </div>
              </div>
            </>
          )}
        </>
      )}
    </div>
  );
}
