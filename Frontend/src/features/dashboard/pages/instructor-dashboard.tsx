import { UsersIcon, BookOpenIcon, FileTextIcon, BarChart3Icon, TrendingUpIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { StatCard } from "@/shared/components/stat-card";
import { PageHeader } from "@/shared/components/page-header";
import { courses } from "@/shared/data/courses";
import { instructors, announcements } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";

export function InstructorDashboard() {
  const { user } = useAuth();
  const currentInstructor = instructors.find((i) => i.id === user?.id) ?? instructors[0];
  const instructorCourses = courses.filter((c) => c.instructorId === currentInstructor.id);
  const totalStudents = instructorCourses.reduce((sum, c) => sum + c.enrolledCount, 0);

  return (
    <div className="space-y-6">
      <PageHeader title={`Welcome back, ${currentInstructor.firstName}!`} description="Here's an overview of your courses and student submissions" actions={<Link to="/instructor/builder"><Button>New Course</Button></Link>} />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Total Students" value={totalStudents} icon={<UsersIcon className="size-5" />} variant="info" trend={{ value: 8, label: "vs last semester" }} />
        <StatCard title="Active Courses" value={instructorCourses.filter((c) => c.status === "published").length} icon={<BookOpenIcon className="size-5" />} variant="success" />
        <StatCard title="Pending Reviews" value={12} icon={<FileTextIcon className="size-5" />} variant="warning" />
        <StatCard title="Average Grade" value="78.5%" icon={<BarChart3Icon className="size-5" />} trend={{ value: 3, label: "vs last month" }} />
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
            <CardContent className="space-y-4">
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
                  <CardTitle>My Courses</CardTitle>
                  <CardDescription>Your courses and their activity</CardDescription>
                </div>
                <Link to="/instructor/courses"><Button variant="outline" size="sm">Manage Courses</Button></Link>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {instructorCourses.slice(0, 3).map((course) => (
                  <div key={course.id} className="flex items-center justify-between p-3 rounded-lg border bg-card">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        <Badge variant="outline">{course.code}</Badge>
                        <span className="font-medium">{course.title}</span>
                      </div>
                      <div className="flex items-center gap-3 text-xs text-muted-foreground">
                        <span>{course.enrolledCount} students</span>
                        <span>/</span>
                        <span>65% avg progress</span>
                      </div>
                    </div>
                    <Progress value={65} className="w-24" />
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader><CardTitle>Quick Stats</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between"><span className="text-sm text-muted-foreground">This Week's Submissions</span><span className="font-bold">47</span></div>
              <div className="flex items-center justify-between"><span className="text-sm text-muted-foreground">Graded This Week</span><span className="font-bold text-success">35</span></div>
              <div className="flex items-center justify-between"><span className="text-sm text-muted-foreground">Office Hours Booked</span><span className="font-bold">8</span></div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader><CardTitle>Recent Announcements</CardTitle></CardHeader>
            <CardContent>
              <div className="space-y-3">
                {announcements.slice(0, 3).map((announcement) => (
                  <div key={announcement.id} className="p-2 rounded bg-muted">
                    <p className="text-sm font-medium line-clamp-1">{announcement.title}</p>
                    <p className="text-xs text-muted-foreground">{announcement.createdAt}</p>
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
