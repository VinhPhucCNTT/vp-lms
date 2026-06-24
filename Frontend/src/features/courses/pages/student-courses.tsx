import * as React from "react";
import { SearchIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/shared/components/page-header";
import { CourseCard } from "@/shared/components/course-card";
import { courses } from "@/shared/data/courses";
import { students } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";

export function StudentCourses() {
  const { user } = useAuth();
  const currentUser = students.find((s) => s.id === user?.id) ?? students[0];
  const [search, setSearch] = React.useState("");

  const enrolledCourses = courses.filter((c) => currentUser.enrolledCourses.includes(c.id) && c.status === "published");
  const filteredCourses = enrolledCourses.filter((course) =>
    course.title.toLowerCase().includes(search.toLowerCase()) || course.code.toLowerCase().includes(search.toLowerCase())
  );

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
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {filteredCourses.map((course) => (
              <CourseCard key={course.id} course={course} progress={Math.floor(Math.random() * 100)} showProgress />
            ))}
          </div>
        </TabsContent>

        <TabsContent value="in-progress" className="mt-6">
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {filteredCourses.map((course) => (
              <CourseCard key={course.id} course={course} progress={Math.floor(Math.random() * 80) + 10} showProgress />
            ))}
          </div>
        </TabsContent>

        <TabsContent value="completed" className="mt-6">
          <div className="text-center py-12 text-muted-foreground">No completed courses yet.</div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
