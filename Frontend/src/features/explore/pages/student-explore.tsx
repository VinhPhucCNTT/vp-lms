import * as React from "react";
import { Link } from "react-router-dom";
import { SearchIcon, BookOpenIcon, UsersIcon } from "lucide-react";
import { Card, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import { courses, departments, semesters, levels } from "@/shared/data/courses";
import { instructors } from "@/shared/data/users";
import { students } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";
import type { Course } from "@/types";

interface ExploreCourseCardProps {
  course: Course;
}

function ExploreCourseCard({ course }: ExploreCourseCardProps) {
  return (
    <Card className="group hover:shadow-md transition-shadow overflow-hidden">
      <div className="h-24 bg-gradient-to-br from-primary/20 to-accent/20 relative flex items-center justify-center">
        <BookOpenIcon className="size-8 text-primary/40" />
      </div>
      <CardHeader className="pb-2">
        <div className="flex items-center gap-2">
          <Badge variant="outline">{course.code}</Badge>
        </div>
        <CardTitle className="text-base line-clamp-1">{course.title}</CardTitle>
        <CardDescription className="line-clamp-2">{course.description}</CardDescription>
      </CardHeader>
      <CardFooter className="pt-2 border-t">
        <div className="flex items-center justify-between w-full">
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <UsersIcon className="size-3" />
            <span>{course.enrolledCount} students</span>
          </div>
          <div className="flex items-center gap-2">
            <Link to={`/student/courses/${course.id}`}>
              <Button size="sm" variant="secondary">View</Button>
            </Link>
          </div>
        </div>
      </CardFooter>
    </Card>
  );
}

export function StudentExplore() {
  const { user } = useAuth();
  const currentUser = students.find((s) => s.id === user?.id) ?? students[0];

  const [search, setSearch] = React.useState("");
  const [semesterFilter, setSemesterFilter] = React.useState<string>("all");
  const [departmentFilter, setDepartmentFilter] = React.useState<string>("all");
  const [levelFilter, setLevelFilter] = React.useState<string>("all");

  const allCourses = courses.filter(
    (c) => c.status === "published" && !currentUser.enrolledCourses.includes(c.id)
  );

  const filteredCourses = React.useMemo(() => {
    return allCourses.filter((course) => {
      const matchesSearch =
        course.title.toLowerCase().includes(search.toLowerCase()) ||
        course.code.toLowerCase().includes(search.toLowerCase()) ||
        course.description.toLowerCase().includes(search.toLowerCase()) ||
        course.tags?.some((tag) => tag.toLowerCase().includes(search.toLowerCase())) ||
        instructors
          .find((i) => i.id === course.instructorId)
          ?.firstName.toLowerCase()
          .includes(search.toLowerCase()) ||
        instructors
          .find((i) => i.id === course.instructorId)
          ?.lastName.toLowerCase()
          .includes(search.toLowerCase());

      const matchesSemester = semesterFilter === "all" || course.semester === semesterFilter;
      const matchesDepartment = departmentFilter === "all" || course.department === departmentFilter;
      const matchesLevel = levelFilter === "all" || course.level === levelFilter;

      return matchesSearch && matchesSemester && matchesDepartment && matchesLevel;
    });
  }, [allCourses, search, semesterFilter, departmentFilter, levelFilter]);

  const featuredCourses = filteredCourses.filter((c) => c.featured).slice(0, 3);

  const coursesByDepartment = React.useMemo(() => {
    const grouped = new Map<string, Course[]>();
    filteredCourses.forEach((course) => {
      const dept = course.department || "Other";
      if (!grouped.has(dept)) {
        grouped.set(dept, []);
      }
      grouped.get(dept)!.push(course);
    });
    return grouped;
  }, [filteredCourses]);

  const recentlyUpdated = [...allCourses]
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    .slice(0, 3);

  const [showLoadMore, setShowLoadMore] = React.useState(false);
  const displayCount = showLoadMore ? filteredCourses.length : 12;

  return (
    <div className="space-y-8">
      <PageHeader
        title="Explore Courses"
        description="Discover new courses to enhance your learning journey"
        breadcrumbs={[
          { label: "Dashboard", href: "/student" },
          { label: "Explore" },
        ]}
      />

      <div className="space-y-4">
        <div className="relative max-w-xl">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input
            placeholder="Search courses, instructors, codes..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
          />
        </div>

        <div className="flex flex-wrap gap-2">
          <Select value={semesterFilter} onValueChange={setSemesterFilter}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Semester" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Semesters</SelectItem>
              {semesters.map((sem) => (
                <SelectItem key={sem} value={sem}>{sem}</SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select value={departmentFilter} onValueChange={setDepartmentFilter}>
            <SelectTrigger className="w-[160px]">
              <SelectValue placeholder="Department" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Departments</SelectItem>
              {departments.map((dept) => (
                <SelectItem key={dept} value={dept}>{dept}</SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select value={levelFilter} onValueChange={setLevelFilter}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Level" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Levels</SelectItem>
              {levels.map((level) => (
                <SelectItem key={level} value={level}>
                  {level.charAt(0).toUpperCase() + level.slice(1)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {featuredCourses.length > 0 && !search && semesterFilter === "all" && departmentFilter === "all" && levelFilter === "all" && (
        <>
          <div>
            <h2 className="text-xl font-semibold mb-4">Featured / Recommended</h2>
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {featuredCourses.map((course) => (
                <ExploreCourseCard key={course.id} course={course} />
              ))}
            </div>
          </div>
          <Separator />
        </>
      )}

      {coursesByDepartment.size > 0 ? (
        Array.from(coursesByDepartment.entries()).map(([department, deptCourses]) => (
          <div key={department}>
            <h2 className="text-xl font-semibold mb-4">Browse by Department &mdash; {department}</h2>
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {deptCourses.slice(0, 3).map((course) => (
                <ExploreCourseCard key={course.id} course={course} />
              ))}
            </div>
            {deptCourses.length > 3 && (
              <Button variant="link" className="mt-2" asChild>
                <Link to={`/student/explore?department=${encodeURIComponent(department)}`}>
                  View all {deptCourses.length} courses in {department}
                </Link>
              </Button>
            )}
          </div>
        ))
      ) : (
        <div className="text-center py-12 text-muted-foreground">
          No courses found matching your criteria.
        </div>
      )}

      {recentlyUpdated.length > 0 && !search && semesterFilter === "all" && departmentFilter === "all" && levelFilter === "all" && (
        <>
          <Separator />
          <div>
            <h2 className="text-xl font-semibold mb-4">Recently Updated</h2>
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {recentlyUpdated.map((course) => (
                <ExploreCourseCard key={course.id} course={course} />
              ))}
            </div>
          </div>
        </>
      )}

      {filteredCourses.length > displayCount && (
        <div className="flex justify-center">
          <Button variant="outline" onClick={() => setShowLoadMore(true)}>
            Load More
          </Button>
        </div>
      )}
    </div>
  );
}
