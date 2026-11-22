import * as React from "react";
import { BookOpenIcon, ChevronLeftIcon, ChevronRightIcon, SearchIcon, UsersIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { PageHeader } from "@/shared/components/page-header";
import { EmptyState, ErrorState, LoadingState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { courseApi, type StudentCourseResponse } from "@/features/courses/api/course-api";

const PAGE_SIZE = 9;

function EnrolledCourseCard({ course }: { course: StudentCourseResponse }) {
  const instructorName = `${course.info.instructorFirstName} ${course.info.instructorLastName}`.trim();

  return (
    <Card className="overflow-hidden">
      <div className="h-28 bg-gradient-to-br from-primary/20 to-accent/20 flex items-center justify-center">
        <BookOpenIcon className="size-10 text-primary/40" />
      </div>
      <CardHeader className="pb-2">
        <div className="flex items-start justify-between gap-3">
          <h2 className="font-semibold line-clamp-1">{course.info.title}</h2>
          <Badge variant="secondary">Enrolled</Badge>
        </div>
        {instructorName && (
          <p className="text-sm text-muted-foreground">Instructor: {instructorName}</p>
        )}
      </CardHeader>
      <CardContent>
        <p className="text-sm text-muted-foreground line-clamp-2">
          {course.info.description || "No description provided."}
        </p>
      </CardContent>
      <CardFooter className="border-t pt-3">
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <UsersIcon className="size-3" />
          <span>{course.info.enrollmentCount} students</span>
        </div>
      </CardFooter>
    </Card>
  );
}

export function StudentCourses() {
  const [page, setPage] = React.useState(1);
  const [search, setSearch] = React.useState("");
  const { data, loading, error, reload } = useApi(
    () => courseApi.getEnrolledCourses(page, PAGE_SIZE),
    [page],
  );

  const filteredCourses = React.useMemo(() => {
    if (!data) return [];
    const query = search.trim().toLowerCase();
    if (!query) return data.data;
    return data.data.filter((course) => {
      const instructor = `${course.info.instructorFirstName} ${course.info.instructorLastName}`;
      return [course.info.title, course.info.description ?? "", instructor]
        .some((value) => value.toLowerCase().includes(query));
    });
  }, [data, search]);

  const totalPages = data ? Math.max(1, Math.ceil(data.maxCount / data.pageSize)) : 1;

  if (loading) return <LoadingState label="Loading your courses..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <PageHeader
        title="My Courses"
        description="Courses you are enrolled in"
        breadcrumbs={[{ label: "Dashboard", href: "/student" }, { label: "My Courses" }]}
      />

      <div className="relative flex-1 max-w-md">
        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
        <Input
          placeholder="Search this page..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          className="pl-9"
        />
      </div>

      {!data || data.maxCount === 0 ? (
        <EmptyState message="You are not enrolled in any courses yet." />
      ) : filteredCourses.length === 0 ? (
        <EmptyState message="No courses on this page match your search." />
      ) : (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {filteredCourses.map((course) => (
            <EnrolledCourseCard key={course.info.id} course={course} />
          ))}
        </div>
      )}

      {data && data.maxCount > PAGE_SIZE && (
        <div className="flex items-center justify-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((current) => Math.max(1, current - 1))}
            disabled={page === 1}
          >
            <ChevronLeftIcon className="size-4" />
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">Page {page} of {totalPages}</span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
            disabled={page >= totalPages}
          >
            Next
            <ChevronRightIcon className="size-4" />
          </Button>
        </div>
      )}
    </div>
  );
}
