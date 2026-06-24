import * as React from "react"
import {
  DownloadIcon,
  UsersIcon,
  BookOpenIcon,
  TrendingUpIcon,
  TrendingDownIcon,
  BarChart3Icon,
  PieChartIcon,
  ActivityIcon,
} from "lucide-react"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { PageHeader } from "@/shared/components/page-header"
import { courses } from "@/shared/data/courses"
import { students, instructors } from "@/shared/data/users"

export function AdminReports() {
  const [dateRange, setDateRange] = React.useState("semester")
  const [reportType, setReportType] = React.useState("overview")

  const stats = {
    totalUsers: students.length + instructors.length,
    activeUsers: Math.floor((students.length + instructors.length) * 0.87),
    totalCourses: courses.length,
    activeCourses: courses.filter((c) => c.status === "published").length,
    totalEnrollments: courses.reduce((sum, c) => sum + c.enrolledCount, 0),
    averageGrade: 78.5,
    completionRate: 72.3,
  }

  const topPerformers = students.slice(0, 5).sort((a, b) => b.gpa - a.gpa)
  const courseStats = courses.slice(0, 5).map((c) => ({
    code: c.code,
    title: c.title,
    enrolled: c.enrolledCount,
    avgGrade: Math.floor(Math.random() * 20) + 65,
    completion: Math.floor(Math.random() * 30) + 60,
  }))

  return (
    <div className="space-y-6">
      <PageHeader
        title="Reports & Analytics"
        description="Platform-wide statistics and performance metrics"
        breadcrumbs={[
          { label: "Dashboard", href: "/admin" },
          { label: "Reports" },
        ]}
        actions={
          <Select value={dateRange} onValueChange={setDateRange}>
            <SelectTrigger className="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="week">This Week</SelectItem>
              <SelectItem value="month">This Month</SelectItem>
              <SelectItem value="semester">This Semester</SelectItem>
              <SelectItem value="year">This Year</SelectItem>
            </SelectContent>
          </Select>
        }
      />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-medium">
              <UsersIcon className="size-4" />
              Active Users
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.activeUsers}</p>
            <p className="text-xs text-muted-foreground">
              of {stats.totalUsers} total
            </p>
            <Progress
              value={(stats.activeUsers / stats.totalUsers) * 100}
              className="mt-2"
            />
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-medium">
              <BookOpenIcon className="size-4" />
              Active Courses
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.activeCourses}</p>
            <p className="text-xs text-muted-foreground">
              of {stats.totalCourses} total
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-medium">
              <TrendingUpIcon className="size-4" />
              Avg. Grade
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.averageGrade}%</p>
            <div className="mt-1 flex items-center gap-1 text-xs text-success">
              <TrendingUpIcon className="size-3" />
              +2.3% from last month
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-medium">
              <ActivityIcon className="size-4" />
              Completion Rate
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.completionRate}%</p>
            <div className="mt-1 flex items-center gap-1 text-xs text-destructive">
              <TrendingDownIcon className="size-3" />
              -1.2% from last month
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="users">
        <TabsList>
          <TabsTrigger value="users">
            <UsersIcon className="mr-2 size-4" />
            User Reports
          </TabsTrigger>
          <TabsTrigger value="courses">
            <BarChart3Icon className="mr-2 size-4" />
            Course Reports
          </TabsTrigger>
          <TabsTrigger value="performance">
            <TrendingUpIcon className="mr-2 size-4" />
            Performance
          </TabsTrigger>
        </TabsList>

        <TabsContent value="users" className="mt-6 grid gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle>User Distribution</CardTitle>
              <CardDescription>Breakdown by role</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                {[
                  {
                    label: "Students",
                    count: students.length,
                    color: "bg-info",
                  },
                  {
                    label: "Instructors",
                    count: instructors.length,
                    color: "bg-success",
                  },
                  { label: "Admins", count: 2, color: "bg-primary" },
                ].map((item) => (
                  <div key={item.label} className="space-y-1">
                    <div className="flex items-center justify-between text-sm">
                      <span className="font-medium">{item.label}</span>
                      <span className="text-muted-foreground">
                        {item.count} (
                        {Math.round((item.count / stats.totalUsers) * 100)}%)
                      </span>
                    </div>
                    <Progress
                      value={(item.count / stats.totalUsers) * 100}
                      className="h-2"
                    />
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Top Performers</CardTitle>
              <CardDescription>Students with highest GPA</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {topPerformers.map((student, index) => (
                <div
                  key={student.id}
                  className="flex items-center justify-between rounded-lg bg-muted p-3"
                >
                  <div className="flex items-center gap-3">
                    <div className="flex size-8 items-center justify-center rounded-full bg-primary/10 font-bold text-primary">
                      {index + 1}
                    </div>
                    <div>
                      <p className="font-medium">
                        {student.firstName} {student.lastName}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {student.studentId}
                      </p>
                    </div>
                  </div>
                  <Badge variant="success" className="font-bold">
                    {student.gpa.toFixed(2)} GPA
                  </Badge>
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="courses" className="mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Course Statistics</CardTitle>
              <CardDescription>Performance metrics by course</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {courseStats.map((course) => (
                  <div key={course.code} className="rounded-lg border p-4">
                    <div className="mb-3 flex items-start justify-between">
                      <div>
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{course.code}</Badge>
                          <span className="font-medium">{course.title}</span>
                        </div>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {course.enrolled} students enrolled
                        </p>
                      </div>
                      <Button size="sm" variant="outline">
                        <DownloadIcon className="mr-2 size-4" />
                        Export
                      </Button>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-1">
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-muted-foreground">
                            Avg. Grade
                          </span>
                          <span className="font-medium">
                            {course.avgGrade}%
                          </span>
                        </div>
                        <Progress value={course.avgGrade} />
                      </div>
                      <div className="space-y-1">
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-muted-foreground">
                            Completion
                          </span>
                          <span className="font-medium">
                            {course.completion}%
                          </span>
                        </div>
                        <Progress value={course.completion} />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent
          value="performance"
          className="mt-6 grid gap-6 lg:grid-cols-2"
        >
          <Card>
            <CardHeader>
              <CardTitle>Platform Engagement</CardTitle>
              <CardDescription>Weekly activity metrics</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {[
                { label: "Logins This Week", value: 892, change: "+12%" },
                { label: "Submissions This Week", value: 156, change: "+8%" },
                { label: "Judge Attempts", value: 47, change: "+23%" },
                { label: "Forum Posts", value: 34, change: "-5%" },
              ].map((item) => (
                <div
                  key={item.label}
                  className="flex items-center justify-between rounded-lg bg-muted p-3"
                >
                  <span className="text-sm font-medium">{item.label}</span>
                  <div className="flex items-center gap-2">
                    <span className="font-bold">{item.value}</span>
                    <Badge
                      variant={
                        item.change.startsWith("+") ? "success" : "destructive"
                      }
                      className="text-xs"
                    >
                      {item.change}
                    </Badge>
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>System Load</CardTitle>
              <CardDescription>Resource utilization</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {[
                { label: "API Requests", value: 45, unit: "k/day" },
                { label: "Judge Engine Queue", value: 12, unit: "jobs" },
                { label: "Storage Used", value: 68, unit: "%" },
                { label: "Database Load", value: 34, unit: "%" },
              ].map((item) => (
                <div key={item.label} className="space-y-1">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-muted-foreground">{item.label}</span>
                    <span className="font-medium">
                      {item.value}
                      {item.unit}
                    </span>
                  </div>
                  <Progress
                    value={
                      typeof item.value === "number" && item.unit === "%"
                        ? item.value
                        : (item.value / 100) * 100
                    }
                  />
                </div>
              ))}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}
