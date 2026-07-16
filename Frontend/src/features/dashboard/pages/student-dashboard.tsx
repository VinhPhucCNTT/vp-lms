import { BookOpenIcon, FileTextIcon, ClipboardCheckIcon, ClockIcon, TrendingUpIcon, CodeIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Button } from "@/components/ui/button";
import { StatCard } from "@/shared/components/stat-card";
import { PageHeader } from "@/shared/components/page-header";
import { CourseCard } from "@/shared/components/course-card";
import { courses, getStudentActivities } from "@/shared/data/courses";
import { students } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";

export function StudentDashboard() {
  const { user } = useAuth();
  const currentUser = students.find((s) => s.id === user?.id) ?? students[0];
  const enrolledCourses = courses.filter((c) => currentUser.enrolledCourses.includes(c.id));
  const studentActivities = getStudentActivities(currentUser.enrolledCourses);

  const pendingAssignments = studentActivities.filter(a => a.type === "assignment" && a.status === "pending").length;
  const pendingAssessments = studentActivities.filter(a => a.type === "assessment" && a.status === "pending").length;
  const pendingCodingProblems = studentActivities.filter(a => a.type === "coding-problem" && a.status === "pending").length;

  return (
    <div className="space-y-6">
      <PageHeader title={`Welcome back, ${currentUser.firstName}!`} description="Here's an overview of your academic progress" />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Active Courses" value={enrolledCourses.length} icon={<BookOpenIcon className="size-5" />} variant="info" />
        <StatCard title="Pending Assignments" value={pendingAssignments} icon={<FileTextIcon className="size-5" />} variant="warning" />
        <StatCard title="Pending Assessments" value={pendingAssessments} icon={<ClipboardCheckIcon className="size-5" />} variant="warning" />
        <StatCard title="Coding Problems" value={pendingCodingProblems} icon={<CodeIcon className="size-5" />} variant="success" />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Due Soon</CardTitle>
                  <CardDescription>Upcoming assignments, assessments, and coding problems</CardDescription>
                </div>
                <Link to="/student/activity"><Button variant="outline" size="sm">View All</Button></Link>
              </div>
            </CardHeader>
            <CardContent className="space-y-4">
              {studentActivities
                .filter(a => a.status === "pending" || a.status === "overdue")
                .slice(0, 6)
                .map((activity) => (
                  <div key={activity.id} className="flex items-center justify-between p-3 rounded-lg border bg-card">
                    <div className="space-y-1">
                      <p className="font-medium text-sm">{activity.title}</p>
                      <div className="flex items-center gap-2">
                        <Badge variant="outline" className="text-xs">{activity.course.code}</Badge>
                        <Badge variant={activity.type === "assignment" ? "secondary" : activity.type === "coding-problem" ? "info" : "info"} className="text-xs">{activity.type}</Badge>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className={`text-sm ${activity.status === "overdue" ? "text-destructive font-medium" : "text-muted-foreground"}`}>{new Date(activity.dueDate).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}</p>
                      {activity.status === "overdue" && <div className="flex items-center gap-1 text-xs text-destructive"><ClockIcon className="size-3" /><span>Overdue</span></div>}
                    </div>
                  </div>
                ))}
            </CardContent>
          </Card>

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
              <div className="grid gap-4 sm:grid-cols-2">
                {enrolledCourses.slice(0, 4).map((course) => (
                  <CourseCard key={course.id} course={course} progress={Math.floor(Math.random() * 100)} showProgress variant="compact" />
                ))}
              </div>
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
              {[{ label: "CS 101", value: 78 }, { label: "CS 201", value: 65 }, { label: "CS 301", value: 42 }].map((item) => (
                <div key={item.label} className="space-y-2">
                  <div className="flex items-center justify-between"><span className="text-sm">{item.label}</span><span className="text-sm font-medium">{item.value}%</span></div>
                  <Progress value={item.value} />
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardHeader><CardTitle>Recent Activity</CardTitle></CardHeader>
            <CardContent>
              <div className="space-y-3">
                {[
                  { action: "Submitted Two Sum", time: "2 hours ago" },
                  { action: "Completed Binary Search", time: "1 day ago" },
                  { action: "Scored 95% on Quiz", time: "2 days ago" },
                  { action: "Enrolled in CS 401", time: "3 days ago" },
                ].map((activity, index) => (
                  <div key={index} className="flex items-center gap-3 text-sm">
                    <div className="size-2 rounded-full bg-primary" />
                    <div className="flex-1"><p>{activity.action}</p><p className="text-xs text-muted-foreground">{activity.time}</p></div>
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
