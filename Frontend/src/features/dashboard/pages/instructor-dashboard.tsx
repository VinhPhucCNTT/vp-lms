import { UsersIcon, BookOpenIcon, FileTextIcon, BarChart3Icon, ClipboardCheckIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { StatCard } from "@/shared/components/stat-card";
import { PageHeader } from "@/shared/components/page-header";
import { courses, courseActivities } from "@/shared/data/courses";
import { instructors, announcements } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";

export function InstructorDashboard() {
  const { user } = useAuth();
  const currentInstructor = instructors.find((i) => i.id === user?.id) ?? instructors[0];
  const instructorCourses = courses.filter((c) => c.instructorId === currentInstructor.id);
  const totalStudents = instructorCourses.reduce((sum, c) => sum + c.enrolledCount, 0);
  const publishedCount = instructorCourses.filter((c) => c.status === "published").length;

  const instructorCourseIds = new Set(instructorCourses.map((c) => c.id));
  const allActivities = courseActivities.filter((a) => instructorCourseIds.has(a.courseId));
  const pendingAssignments = allActivities.filter((a) => a.type === "assignment").length;
  const pendingAssessments = allActivities.filter((a) => a.type === "assessment").length;
  const pendingProblems = allActivities.filter((a) => a.type === "coding-problem").length;
  const totalPending = pendingAssignments + pendingAssessments + pendingProblems;

  const recentCourses = instructorCourses.slice(0, 3);

  return (
    <div className="space-y-6">
      <PageHeader
        title={`Welcome back, ${currentInstructor.firstName}!`}
        description="Here's an overview of your courses and student submissions"
        actions={<Link to="/instructor/courses"><Button>New Course</Button></Link>}
      />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title="Total Students"
          value={totalStudents}
          icon={<UsersIcon className="size-5" />}
          variant="info"
          trend={{ value: 8, label: "vs last semester" }}
        />
        <StatCard
          title="Active Courses"
          value={`${publishedCount} / ${instructorCourses.length}`}
          icon={<BookOpenIcon className="size-5" />}
          variant="success"
          description="published / total"
        />
        <Card>
          <CardContent className="p-6">
            <div className="flex items-start justify-between">
              <div className="space-y-2">
                <p className="text-sm text-muted-foreground">Pending Review</p>
                <p className="text-2xl font-bold">{totalPending}</p>
                <div className="flex items-center gap-2 flex-wrap">
                  <Badge variant="outline" className="text-xs gap-1">
                    <FileTextIcon className="size-3" />
                    {pendingAssignments} assignments
                  </Badge>
                  <Badge variant="outline" className="text-xs gap-1">
                    <ClipboardCheckIcon className="size-3" />
                    {pendingAssessments} quizzes
                  </Badge>
                </div>
              </div>
              <div className="size-10 rounded-lg bg-warning/10 flex items-center justify-center">
                <FileTextIcon className="size-5 text-warning-foreground" />
              </div>
            </div>
          </CardContent>
        </Card>
        <StatCard
          title="Graded This Week"
          value={35}
          icon={<BarChart3Icon className="size-5" />}
          trend={{ value: 3, label: "vs last week" }}
        />
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
              {[
                { student: "Alex Chen", assignment: "Algorithm Analysis Practice", course: "CS 101", submitted: "2 hours ago", urgent: true },
                { student: "Sarah Johnson", assignment: "Sorting Implementation", course: "CS 201", submitted: "1 day ago", urgent: false },
                { student: "Michael Brown", assignment: "Database Design Project", course: "CS 301", submitted: "2 days ago", urgent: false },
              ].map((item, index) => (
                <div key={index} className="flex items-center justify-between p-3 rounded-lg border bg-card">
                  <div className="space-y-1">
                    <p className="font-medium text-sm">{item.student}</p>
                    <p className="text-sm text-muted-foreground">{item.assignment}</p>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className="text-xs">{item.course}</Badge>
                      <span className="text-xs text-muted-foreground">{item.submitted}</span>
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
                  <CardTitle>Recently Accessed Courses</CardTitle>
                  <CardDescription>Your most recently active courses</CardDescription>
                </div>
                <Link to="/instructor/courses"><Button variant="outline" size="sm">All Courses</Button></Link>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {recentCourses.map((course, idx) => {
                  const lastAccessed = ["Today", "1 day ago", "2 days ago"][idx];
                  const activitiesForCourse = allActivities.filter((a) => a.courseId === course.id);
                  return (
                    <div key={course.id} className="flex items-center justify-between p-3 rounded-lg border bg-card">
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{course.code}</Badge>
                          <span className="font-medium">{course.title}</span>
                        </div>
                        <div className="flex items-center gap-3 text-xs text-muted-foreground">
                          <span>{course.enrolledCount} students</span>
                          <span className="text-border">·</span>
                          <span>{activitiesForCourse.length} activities</span>
                          <span className="text-border">·</span>
                          <span>Accessed {lastAccessed}</span>
                        </div>
                      </div>
                      <Badge variant={course.status === "published" ? "success" : "secondary"} className="shrink-0">
                        {course.status}
                      </Badge>
                    </div>
                  );
                })}
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
                {announcements.slice(0, 3).map((announcement) => (
                  <div key={announcement.id} className="p-3 rounded-lg bg-muted">
                    <p className="text-sm font-medium line-clamp-2">{announcement.title}</p>
                    <p className="text-xs text-muted-foreground mt-1">{announcement.createdAt}</p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
