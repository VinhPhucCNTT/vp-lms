import { BookOpenIcon, UsersIcon } from "lucide-react";
import { Link } from "react-router-dom";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Button } from "@/components/ui/button";
import type { Course } from "@/types";
import { instructors } from "@/shared/data/users";

interface CourseCardProps {
  course: Course;
  progress?: number;
  showProgress?: boolean;
  variant?: "default" | "compact";
}

export function CourseCard({ course, progress, showProgress = false, variant = "default" }: CourseCardProps) {
  const instructor = instructors.find((i) => i.id === course.instructorId);

  if (variant === "compact") {
    return (
      <Card className="group hover:shadow-md transition-shadow">
        <CardHeader className="p-4 pb-2">
          <div className="flex items-start justify-between">
            <div className="space-y-1">
              <Badge variant="secondary" className="text-xs">{course.code}</Badge>
              <h3 className="font-semibold leading-tight line-clamp-1">{course.title}</h3>
            </div>
            <Badge variant={course.status === "published" ? "success" : "secondary"}>{course.status}</Badge>
          </div>
        </CardHeader>
        <CardContent className="p-4 pt-0">
          <p className="text-sm text-muted-foreground line-clamp-2">{course.description}</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="group hover:shadow-md transition-shadow overflow-hidden">
      <div className="h-32 bg-gradient-to-br from-primary/20 to-accent/20 relative">
        <div className="absolute inset-0 flex items-center justify-center">
          <BookOpenIcon className="size-12 text-primary/40" />
        </div>
        <Badge className="absolute top-3 left-3" variant={course.status === "published" ? "success" : "secondary"}>{course.status}</Badge>
      </div>
      <CardHeader className="pb-2">
        <div className="flex items-center gap-2">
          <Badge variant="outline">{course.code}</Badge>
        </div>
        <h3 className="font-semibold line-clamp-1">{course.title}</h3>
        {instructor && <p className="text-sm text-muted-foreground">{instructor.firstName} {instructor.lastName}</p>}
      </CardHeader>
      <CardContent className="pb-2">
        <p className="text-sm text-muted-foreground line-clamp-2">{course.description}</p>
        {showProgress && progress !== undefined && (
          <div className="mt-3 space-y-1">
            <div className="flex items-center justify-between text-xs text-muted-foreground">
              <span>Progress</span>
              <span>{progress}%</span>
            </div>
            <Progress value={progress} />
          </div>
        )}
      </CardContent>
      <CardFooter className="pt-2 border-t">
        <div className="flex items-center justify-between w-full">
          <div className="flex items-center gap-1 text-xs text-muted-foreground">
            <UsersIcon className="size-3" />
            <span>{course.enrolledCount} students</span>
          </div>
          <Link to={`/student/courses/${course.id}`}>
            <Button size="sm" variant="secondary">View Course</Button>
          </Link>
        </div>
      </CardFooter>
    </Card>
  );
}
