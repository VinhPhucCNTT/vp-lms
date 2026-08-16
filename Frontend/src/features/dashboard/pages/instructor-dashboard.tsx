import { UsersIcon, BookOpenIcon, FileTextIcon, BarChart3Icon, ClipboardCheckIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { StatCard } from "@/shared/components/stat-card";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { useAuth } from "@/features/auth/auth-context";
import { instructorApi, type InstructorStatsDto, type PendingSubmissionDto } from "@/features/courses/instructor-api";
import { courseApi } from "@/features/courses/course-api";
import type { Course } from "@/types";

export function InstructorDashboard() {
  const { user } = useAuth();
  const { data: stats, loading, error, reload } = useApi<InstructorStatsDto>(() => instructorApi.getStats());
  const { data: pendingSubs } = useApi<PendingSubmissionDto[]>(() => instructorApi.getPendingSubmissions());
  const { data: recentCourses } = useApi<Course[]>(() => instructorApi.getRecentCourses());

  if (loading) return <LoadingState label="Loading dashboard..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;
  if (!stats) return null;

  return (
    <div className="space-y-6">
      <PageHeader
        title={`Welcome back, ${user?.firstName ?? "Instructor"}!`}
        description="Here's an overview of your courses and student submissions"
        actions={<Link to="/instructor/courses"><Button>New Course</Button></Link>}
      />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Total Students" value={stats.totalStudents} icon={<UsersIcon className="size-5" />} variant="info" />
        <StatCard title="Active Courses" value={`${stats.publishedCourses} / ${stats.totalCourses}`} icon={<BookOpenIcon className="size-5" />} variant="success" description="published / total" />
        <Card>
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-2">
                <p className="text-sm text-muted-foreground">Pending Review</p>
                <p className="text-2xl font-bold">{stats.pendingSubmissions}</p>
                <div className="flex items-center gap-2 flex-wrap">
                  <Badge variant="outline" className="text-xs gap-1"><FileTextIcon className="size-3" />{stats.pendingAssignments} assignments</Badge>
                  <Badge variant="outline" className="text-xs gap-1"><ClipboardCheckIcon className="size-3" />{stats.pendingAssessments} quizzes</Badge>
                </div>
              </div>
              <div className="size-10 rounded-lg bg-warning/10 flex items-center justify-center"><FileTextIcon className="size-5 text-warning-foreground" /></div>
            </div>
          </CardContent>
        </Card>
        <StatCard title="Graded This Week" value={stats.gradedThisWeek} icon={<BarChart3Icon className="size-5" />} />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Pending Submissions</CardTitle>
                  <CardDescription>Assignments waiting to be graded</CardDescription>
                </div>
                <Link to="/instructor/submissions"><Button variant="outline" size="sm">View All</Button></Link>
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              {(!pendingSubs || pendingSubs.length === 0) && (
                <p className="text-sm text-muted-foreground text-center py-4">No pending submissions.</p>
              )}
              {pendingSubs?.map((item) => (
                <div key={item.id} className="flex items-center justify-between p-3 rounded-lg border bg-card">
                  <div className="space-y-1">
                    <p className="font-medium text-sm">{item.studentName}</p>
                    <p className="text-sm text-muted-foreground">{item.assignmentTitle}</p>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-xs">{item.courseCode}</Badge>
                      <span className="text-xs text-muted-foreground">{item.submittedAt}</span>
                    </div>
                  </div>
                  <Button size="sm" variant={item.urgent ? "default" : "outline"}>Grade</Button>
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Recent Courses</CardTitle>
                  <CardDescription>Your most recently active courses</CardDescription>
                </div>
                <Link to="/instructor/courses"><Button variant="outline" size="sm">All Courses</Button></Link>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {(!recentCourses || recentCourses.length === 0) && (
                  <p className="text-sm text-muted-foreground text-center py-4">No courses yet.</p>
                )}
                {recentCourses?.slice(0, 3).map((course) => (
                  <div key={course.id} className="flex items-center justify-between p-3 rounded-lg border bg-card">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        <Badge variant="outline">{course.code}</Badge>
                        <span className="font-medium">{course.title}</span>
                      </div>
                      <div className="flex items-center gap-3 text-xs text-muted-foreground">
                        <span>{course.enrolledCount} students</span>
                        <span className="text-border">·</span>
                        <span>{course.status}</span>
                      </div>
                    </div>
                    <Badge variant={course.status === "published" ? "success" : "secondary"} className="shrink-0">{course.status}</Badge>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        <div>
          <Card>
            <CardHeader>
              <CardTitle>Recent Announcements</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <Link to="/instructor/announcements"><Button variant="outline" size="sm" className="w-full">Manage Announcements</Button></Link>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
