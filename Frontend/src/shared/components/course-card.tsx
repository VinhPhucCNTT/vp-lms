import { BookOpenIcon, UsersIcon } from "lucide-react"
import { Link } from "react-router-dom"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Progress } from "@/components/ui/progress"
import { Button } from "@/components/ui/button"
import type { Course } from "@/types"
import { instructors } from "@/shared/data/users"

interface CourseCardProps {
  course: Course
  progress?: number
  showProgress?: boolean
  variant?: "default" | "compact"
}

export function CourseCard({
  course,
  progress,
  showProgress = false,
  variant = "default",
}: CourseCardProps) {
  const instructor = instructors.find((i) => i.id === course.instructorId)

  if (variant === "compact") {
    return (
      <Card className="group transition-shadow hover:shadow-md">
        <CardHeader className="p-4 pb-2">
          <div className="flex items-start justify-between">
            <div className="space-y-1">
              <Badge variant="secondary" className="text-xs">
                {course.code}
              </Badge>
              <h3 className="line-clamp-1 leading-tight font-semibold">
                {course.title}
              </h3>
            </div>
            <Badge
              variant={course.status === "published" ? "success" : "secondary"}
            >
              {course.status}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="p-4 pt-0">
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {course.description}
          </p>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card className="group overflow-hidden transition-shadow hover:shadow-md">
      <div className="relative h-32 bg-gradient-to-br from-primary/20 to-accent/20">
        <div className="absolute inset-0 flex items-center justify-center">
          <BookOpenIcon className="size-12 text-primary/40" />
        </div>
        <Badge
          className="absolute top-3 left-3"
          variant={course.status === "published" ? "success" : "secondary"}
        >
          {course.status}
        </Badge>
      </div>
      <CardHeader className="pb-2">
        <div className="flex items-center gap-2">
          <Badge variant="outline">{course.code}</Badge>
        </div>
        <h3 className="line-clamp-1 font-semibold">{course.title}</h3>
        {instructor && (
          <p className="text-sm text-muted-foreground">
            {instructor.firstName} {instructor.lastName}
          </p>
        )}
      </CardHeader>
      <CardContent className="pb-2">
        <p className="line-clamp-2 text-sm text-muted-foreground">
          {course.description}
        </p>
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
      <CardFooter className="border-t pt-2">
        <div className="flex w-full items-center justify-between">
          <div className="flex items-center gap-1 text-xs text-muted-foreground">
            <UsersIcon className="size-3" />
            <span>{course.enrolledCount} students</span>
          </div>
          <Link to={`/student/courses/${course.id}`}>
            <Button size="sm" variant="secondary">
              View Course
            </Button>
          </Link>
        </div>
      </CardFooter>
    </Card>
  )
}
