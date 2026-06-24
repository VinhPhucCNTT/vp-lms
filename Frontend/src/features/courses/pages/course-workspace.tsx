import * as React from "react"
import { useParams, Link } from "react-router-dom"
import {
  BookOpenIcon,
  FileTextIcon,
  ClipboardCheckIcon,
  MegaphoneIcon,
  BarChart3Icon,
  ChevronRightIcon,
  PlayCircleIcon,
  FileText,
} from "lucide-react"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Progress } from "@/components/ui/progress"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Separator } from "@/components/ui/separator"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { cn } from "@/lib/utils"
import {
  courses,
  modules,
  lessons,
  assignments,
  assessments,
} from "@/shared/data/courses"
import { announcements } from "@/shared/data/users"
import { instructors } from "@/shared/data/users"

const lessonIcons: Record<string, React.ReactNode> = {
  video: <PlayCircleIcon className="size-4" />,
  reading: <FileText className="size-4" />,
  interactive: <BookOpenIcon className="size-4" />,
}

export function CourseWorkspace() {
  const { courseId } = useParams<{ courseId: string }>()
  const course = courses.find((c) => c.id === courseId) ?? courses[0]
  const courseModules = modules.filter((m) => m.courseId === course.id)
  const courseAnnouncements = announcements.filter(
    (a) => a.courseId === course.id
  )
  const instructor = instructors.find((i) => i.id === course.instructorId)

  const [activeTab, setActiveTab] = React.useState("content")
  const [selectedModule, setSelectedModule] = React.useState<string>(
    courseModules[0]?.id ?? ""
  )
  const [selectedLesson, setSelectedLesson] = React.useState<string>(
    lessons[0]?.id ?? ""
  )

  const currentLesson = lessons.find((l) => l.id === selectedLesson)
  const moduleAssignments = assignments.filter(
    (a) => a.moduleId === selectedModule
  )
  const moduleAssessments = assessments.filter(
    (a) => a.moduleId === selectedModule
  )

  const progress = 65

  return (
    <div className="flex h-[calc(100vh-4rem)] flex-col">
      <div className="border-b bg-card">
        <div className="flex items-center justify-between p-4">
          <div className="flex items-center gap-4">
            <Badge variant="outline">{course.code}</Badge>
            <div>
              <h1 className="text-xl font-bold">{course.title}</h1>
              <p className="text-sm text-muted-foreground">
                {instructor &&
                  `Instructor: ${instructor.firstName} ${instructor.lastName}`}
              </p>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <div className="text-right">
              <p className="text-sm text-muted-foreground">Course Progress</p>
              <p className="text-lg font-bold">{progress}%</p>
            </div>
            <Progress value={progress} className="w-32" />
          </div>
        </div>
      </div>

      <div className="flex flex-1 overflow-hidden">
        <div className="flex w-72 flex-col border-r bg-muted/30">
          <div className="border-b bg-card p-3">
            <h2 className="text-sm font-semibold">Course Content</h2>
          </div>
          <ScrollArea className="flex-1">
            <div className="space-y-2 p-2">
              {courseModules.map((module) => (
                <div key={module.id}>
                  <button
                    onClick={() => setSelectedModule(module.id)}
                    className={cn(
                      "w-full rounded-lg p-2 text-left transition-colors",
                      selectedModule === module.id
                        ? "bg-primary text-primary-foreground"
                        : "hover:bg-muted"
                    )}
                  >
                    <div className="flex items-center gap-2">
                      <ChevronRightIcon
                        className={cn(
                          "size-4 transition-transform",
                          selectedModule === module.id && "rotate-90"
                        )}
                      />
                      <span className="text-sm font-medium">
                        {module.title}
                      </span>
                    </div>
                  </button>
                  {selectedModule === module.id && (
                    <div className="mt-1 ml-4 space-y-1">
                      {lessons
                        .filter((l) => l.moduleId === module.id)
                        .map((lesson) => (
                          <button
                            key={lesson.id}
                            onClick={() => {
                              setSelectedLesson(lesson.id)
                              setActiveTab("content")
                            }}
                            className={cn(
                              "flex w-full items-center gap-2 rounded-lg p-2 text-left text-sm transition-colors",
                              selectedLesson === lesson.id
                                ? "bg-accent text-accent-foreground"
                                : "hover:bg-muted"
                            )}
                          >
                            {lessonIcons[lesson.type]}
                            <span className="truncate">{lesson.title}</span>
                          </button>
                        ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          </ScrollArea>
        </div>

        <div className="flex flex-1 flex-col overflow-hidden">
          <Tabs
            value={activeTab}
            onValueChange={setActiveTab}
            className="flex flex-1 flex-col"
          >
            <div className="border-b px-4">
              <TabsList className="h-12" variant="line">
                <TabsTrigger value="content">
                  <BookOpenIcon className="mr-2 size-4" />
                  Content
                </TabsTrigger>
                <TabsTrigger value="assignments">
                  <FileTextIcon className="mr-2 size-4" />
                  Assignments
                </TabsTrigger>
                <TabsTrigger value="assessments">
                  <ClipboardCheckIcon className="mr-2 size-4" />
                  Assessments
                </TabsTrigger>
                <TabsTrigger value="announcements">
                  <MegaphoneIcon className="mr-2 size-4" />
                  Announcements
                  {courseAnnouncements.length > 0 && (
                    <Badge variant="destructive" className="ml-2 text-xs">
                      {courseAnnouncements.length}
                    </Badge>
                  )}
                </TabsTrigger>
                <TabsTrigger value="progress">
                  <BarChart3Icon className="mr-2 size-4" />
                  Progress
                </TabsTrigger>
              </TabsList>
            </div>

            <ScrollArea className="flex-1">
              <div className="p-6">
                <TabsContent value="content" className="mt-0">
                  {currentLesson && (
                    <div className="space-y-6">
                      <div>
                        <Badge variant="outline" className="mb-2">
                          {currentLesson.type}
                        </Badge>
                        <h2 className="text-2xl font-bold">
                          {currentLesson.title}
                        </h2>
                      </div>
                      <div
                        className="prose prose-sm dark:prose-invert max-w-none"
                        dangerouslySetInnerHTML={{
                          __html: currentLesson.content,
                        }}
                      />
                      <div className="flex items-center gap-4 pt-6">
                        <Button variant="outline" disabled>
                          Previous Lesson
                        </Button>
                        <Button>Mark as Complete</Button>
                        <Button variant="outline">Next Lesson</Button>
                      </div>
                    </div>
                  )}
                </TabsContent>

                <TabsContent value="assignments" className="mt-0">
                  <div className="space-y-4">
                    <h2 className="text-xl font-bold">Course Assignments</h2>
                    {moduleAssignments.map((assignment) => (
                      <Card key={assignment.id}>
                        <CardHeader>
                          <div className="flex items-start justify-between">
                            <div>
                              <CardTitle className="text-lg">
                                {assignment.title}
                              </CardTitle>
                              <CardDescription>
                                Due: {assignment.dueDate} / Max Score:{" "}
                                {assignment.maxScore}
                              </CardDescription>
                            </div>
                            <Badge>{assignment.weight}% weight</Badge>
                          </div>
                        </CardHeader>
                        <CardContent>
                          <p className="line-clamp-2 text-sm text-muted-foreground">
                            {assignment.description}
                          </p>
                          <div className="mt-4">
                            <Button size="sm">View Details</Button>
                          </div>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                </TabsContent>

                <TabsContent value="assessments" className="mt-0">
                  <div className="space-y-4">
                    <h2 className="text-xl font-bold">Course Assessments</h2>
                    {moduleAssessments.map((assessment) => (
                      <Card key={assessment.id}>
                        <CardHeader>
                          <div className="flex items-start justify-between">
                            <div>
                              <CardTitle className="text-lg">
                                {assessment.title}
                              </CardTitle>
                              <CardDescription>
                                {assessment.duration} minutes /{" "}
                                {assessment.maxAttempts} attempt(s)
                              </CardDescription>
                            </div>
                            <Badge variant="info">Quiz</Badge>
                          </div>
                        </CardHeader>
                        <CardContent>
                          <p className="text-sm text-muted-foreground">
                            {assessment.description}
                          </p>
                          <div className="mt-4 flex items-center gap-2">
                            <Button size="sm">Start Assessment</Button>
                            <span className="text-sm text-muted-foreground">
                              Due: {assessment.dueDate}
                            </span>
                          </div>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                </TabsContent>

                <TabsContent value="announcements" className="mt-0">
                  <div className="space-y-4">
                    <h2 className="text-xl font-bold">Course Announcements</h2>
                    {courseAnnouncements.map((announcement) => (
                      <Card key={announcement.id}>
                        <CardHeader>
                          {announcement.isPinned && (
                            <Badge variant="secondary" className="mb-2 w-fit">
                              Pinned
                            </Badge>
                          )}
                          <CardTitle className="text-lg">
                            {announcement.title}
                          </CardTitle>
                          <CardDescription>
                            {announcement.createdAt}
                          </CardDescription>
                        </CardHeader>
                        <CardContent>
                          <p className="text-sm">{announcement.content}</p>
                        </CardContent>
                      </Card>
                    ))}
                    {courseAnnouncements.length === 0 && (
                      <Card>
                        <CardContent className="py-8 text-center text-muted-foreground">
                          No announcements yet.
                        </CardContent>
                      </Card>
                    )}
                  </div>
                </TabsContent>

                <TabsContent value="progress" className="mt-0">
                  <div className="space-y-6">
                    <h2 className="text-xl font-bold">Your Progress</h2>
                    <div className="grid gap-4 md:grid-cols-3">
                      <Card>
                        <CardContent className="pt-6">
                          <div className="text-center">
                            <p className="text-3xl font-bold">4</p>
                            <p className="text-sm text-muted-foreground">
                              Completed Lessons
                            </p>
                          </div>
                        </CardContent>
                      </Card>
                      <Card>
                        <CardContent className="pt-6">
                          <div className="text-center">
                            <p className="text-3xl font-bold">8</p>
                            <p className="text-sm text-muted-foreground">
                              Remaining Items
                            </p>
                          </div>
                        </CardContent>
                      </Card>
                      <Card>
                        <CardContent className="pt-6">
                          <div className="text-center">
                            <p className="text-3xl font-bold">{progress}%</p>
                            <p className="text-sm text-muted-foreground">
                              Overall Progress
                            </p>
                          </div>
                        </CardContent>
                      </Card>
                    </div>
                  </div>
                </TabsContent>
              </div>
            </ScrollArea>
          </Tabs>
        </div>
      </div>
    </div>
  )
}
