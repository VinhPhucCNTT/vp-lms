import { UsersIcon, BookOpenIcon, FileTextIcon, ActivityIcon, TrendingUpIcon, ShieldCheckIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { StatCard } from "@/shared/components/stat-card";
import { PageHeader } from "@/shared/components/page-header";
import { courses } from "@/shared/data/courses";
import { students, instructors, admins } from "@/shared/data/users";

export function AdminDashboard() {
  const totalUsers = students.length + instructors.length + admins.length;
  const publishedCourses = courses.filter((c) => c.status === "published").length;
  const totalEnrollment = courses.reduce((sum, c) => sum + c.enrolledCount, 0);

  return (
    <div className="space-y-6">
      <PageHeader title="Admin Dashboard" description="Platform overview and administrative controls" />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Total Users" value={totalUsers} icon={<UsersIcon className="size-5" />} variant="info" trend={{ value: 12, label: "vs last month" }} />
        <StatCard title="Published Courses" value={publishedCourses} icon={<BookOpenIcon className="size-5" />} variant="success" />
        <StatCard title="Total Enrollments" value={totalEnrollment} icon={<FileTextIcon className="size-5" />} />
        <StatCard title="System Health" value="99.9%" icon={<ActivityIcon className="size-5" />} variant="success" />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Platform Statistics</CardTitle>
              <CardDescription>Monthly platform activity</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid gap-4 sm:grid-cols-3">
                <div className="text-center p-4 bg-muted rounded-lg"><p className="text-2xl font-bold">{students.length}</p><p className="text-sm text-muted-foreground">Students</p></div>
                <div className="text-center p-4 bg-muted rounded-lg"><p className="text-2xl font-bold">{instructors.length}</p><p className="text-sm text-muted-foreground">Instructors</p></div>
                <div className="text-center p-4 bg-muted rounded-lg"><p className="text-2xl font-bold">{admins.length}</p><p className="text-sm text-muted-foreground">Admins</p></div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Recent Audit Activity</CardTitle>
                  <CardDescription>System changes and user actions</CardDescription>
                </div>
                <Link to="/admin/audit"><Button variant="outline" size="sm">Full Audit Log</Button></Link>
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              {[
                { user: "Dr. Smith", action: "Updated course CS 101", time: "10 minutes ago" },
                { user: "Admin", action: "Created user account", time: "1 hour ago" },
                { user: "Prof. Johnson", action: "Published announcement", time: "2 hours ago" },
                { user: "System", action: "Automated backup completed", time: "3 hours ago" },
              ].map((item, index) => (
                <div key={index} className="flex items-center justify-between p-3 rounded-lg border bg-card">
                  <div className="space-y-1">
                    <p className="text-sm font-medium">{item.action}</p>
                    <p className="text-xs text-muted-foreground">by {item.user}</p>
                  </div>
                  <span className="text-xs text-muted-foreground">{item.time}</span>
                </div>
              ))}
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2"><ShieldCheckIcon className="size-5" />Quick Actions</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <Link to="/admin/users"><Button variant="outline" className="w-full justify-start"><UsersIcon className="size-4 mr-2" />Manage Users</Button></Link>
              <Link to="/admin/courses"><Button variant="outline" className="w-full justify-start"><BookOpenIcon className="size-4 mr-2" />Course Oversight</Button></Link>
              <Link to="/admin/reports"><Button variant="outline" className="w-full justify-start"><TrendingUpIcon className="size-4 mr-2" />Generate Reports</Button></Link>
            </CardContent>
          </Card>

          <Card>
            <CardHeader><CardTitle>System Status</CardTitle></CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center justify-between"><span className="text-sm">API Server</span><Badge variant="success">Healthy</Badge></div>
              <div className="flex items-center justify-between"><span className="text-sm">Database</span><Badge variant="success">Healthy</Badge></div>
              <div className="flex items-center justify-between"><span className="text-sm">Storage</span><Badge variant="warning">85% Used</Badge></div>
              <div className="flex items-center justify-between"><span className="text-sm">Judge Engine</span><Badge variant="success">Healthy</Badge></div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
