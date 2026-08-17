import { Link, useParams } from "react-router-dom";
import { ArrowLeftIcon } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useApi } from "@/lib/use-api";
import { courseApi } from "@/features/courses/course-api";
import { ErrorState, LoadingState } from "@/shared/components/api-states";

export function InstructorCourseDetail() {
  const { courseId } = useParams<{ courseId: string }>();
  const { data, loading, error, reload } = useApi(
    () => courseApi.getCourseDetail(courseId ?? ""),
    [courseId],
  );

  if (loading) return <LoadingState label="Loading course..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;
  if (!data) return <ErrorState message="Course details are unavailable." onRetry={reload} />;

  return (
    <div className="space-y-6">
      <Link to="/instructor/courses" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeftIcon className="size-4" /> Back to My Courses
      </Link>
      <div>
        <div className="flex items-center gap-2">
          <Badge variant="outline">{data.course.code}</Badge>
          <Badge variant="secondary">Course</Badge>
        </div>
        <h1 className="mt-2 text-2xl font-bold tracking-tight">{data.course.title}</h1>
        <p className="mt-2 text-muted-foreground">{data.course.description || "No description provided."}</p>
      </div>
      <Card>
        <CardHeader><CardTitle>Modules</CardTitle></CardHeader>
        <CardContent className="text-sm text-muted-foreground">
          {data.modules.length === 0 ? "No modules have been added yet." : `${data.modules.length} module${data.modules.length === 1 ? "" : "s"}`}
        </CardContent>
      </Card>
    </div>
  );
}
