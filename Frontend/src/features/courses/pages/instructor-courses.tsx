import * as React from "react";
import { PlusIcon, SearchIcon, UsersIcon, LayersIcon, BookOpenIcon } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { PageHeader } from "@/shared/components/page-header";
import { courses, courseActivities } from "@/shared/data/courses";
import { instructors } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";

export function InstructorCourses() {
  const { user } = useAuth();
  const currentInstructor = instructors.find((i) => i.id === user?.id) ?? instructors[0];
  const [search, setSearch] = React.useState("");

  const instructorCourses = courses.filter((c) => c.instructorId === currentInstructor.id);

  const filterCourses = (status?: "published" | "draft") => {
    let list = status ? instructorCourses.filter((c) => c.status === status) : instructorCourses;
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter((c) => c.title.toLowerCase().includes(q) || c.code.toLowerCase().includes(q));
    }
    return list;
  };

  const all = filterCourses();
  const published = filterCourses("published");
  const drafts = filterCourses("draft");

  function CourseRow({ courseId }: { courseId: string }) {
    const course = instructorCourses.find((c) => c.id === courseId)!;
    const activities = courseActivities.filter((a) => a.courseId === course.id);

    return (
      <Card className="group">
        <CardHeader className="pb-2">
          <div className="flex items-start justify-between gap-4">
            <div className="space-y-1 min-w-0">
              <div className="flex items-center gap-2">
                <Badge variant="outline" className="text-xs shrink-0">{course.code}</Badge>
                <Badge variant={course.status === "published" ? "success" : "secondary"} className="text-xs shrink-0">{course.status}</Badge>
              </div>
              <h3 className="font-semibold leading-tight">{course.title}</h3>
            </div>
          </div>
          <p className="text-sm text-muted-foreground line-clamp-2">{course.description}</p>
        </CardHeader>
        <CardContent className="pb-2">
          <div className="flex items-center gap-4 text-xs text-muted-foreground">
            <div className="flex items-center gap-1">
              <UsersIcon className="size-3" />
              <span>{course.enrolledCount} students</span>
            </div>
            <div className="flex items-center gap-1">
              <LayersIcon className="size-3" />
              <span>{activities.length} activities</span>
            </div>
            <div className="flex items-center gap-1">
              <BookOpenIcon className="size-3" />
              <span>{course.credits} credits</span>
            </div>
          </div>
        </CardContent>
        <CardFooter className="pt-2 border-t">
          <div className="flex items-center gap-2 ml-auto">
            {course.status === "draft" && (
              <Button size="sm" variant="default">Publish</Button>
            )}
            <Button size="sm" variant="outline">Edit</Button>
            <Button size="sm" variant="secondary">View Course</Button>
          </div>
        </CardFooter>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="My Courses"
        description="Courses you have created and manage"
        breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "My Courses" }]}
        actions={
          <Button>
            <PlusIcon className="size-4 mr-2" />
            Create Course
          </Button>
        }
      />

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input
            placeholder="Search courses..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
          />
        </div>
      </div>

      <Tabs defaultValue="all">
        <TabsList>
          <TabsTrigger value="all">All ({all.length})</TabsTrigger>
          <TabsTrigger value="published">Published ({published.length})</TabsTrigger>
          <TabsTrigger value="draft">Unpublished ({drafts.length})</TabsTrigger>
        </TabsList>

        <TabsContent value="all" className="mt-6">
          {all.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">No courses found.</div>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {all.map((c) => <CourseRow key={c.id} courseId={c.id} />)}
            </div>
          )}
        </TabsContent>

        <TabsContent value="published" className="mt-6">
          {published.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">No published courses.</div>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {published.map((c) => <CourseRow key={c.id} courseId={c.id} />)}
            </div>
          )}
        </TabsContent>

        <TabsContent value="draft" className="mt-6">
          {drafts.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">No unpublished courses.</div>
          ) : (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {drafts.map((c) => <CourseRow key={c.id} courseId={c.id} />)}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
