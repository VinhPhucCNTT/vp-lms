import * as React from "react"
import {
  PlusCircleIcon,
  PencilIcon,
  Trash2Icon,
  SendIcon,
  MegaphoneIcon,
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
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Separator } from "@/components/ui/separator"
import { PageHeader } from "@/shared/components/page-header"
import { announcements } from "@/shared/data/users"
import { courses } from "@/shared/data/courses"
import { instructors } from "@/shared/data/users"
import { cn } from "@/lib/utils"
import { useAuth } from "@/features/auth/auth-context"

interface AnnouncementFormData {
  title: string
  content: string
  courseId: string
  isPinned: boolean
}

export function InstructorAnnouncements() {
  const { user } = useAuth()
  const currentInstructor =
    instructors.find((i) => i.id === user?.id) ?? instructors[0]
  const instructorCourses = courses.filter(
    (c) => c.instructorId === currentInstructor.id
  )
  const [isCreateOpen, setIsCreateOpen] = React.useState(false)
  const [selectedCourse, setSelectedCourse] = React.useState<string>("all")

  const [formData, setFormData] = React.useState<AnnouncementFormData>({
    title: "",
    content: "",
    courseId: instructorCourses[0]?.id ?? "",
    isPinned: false,
  })

  const instructorAnnouncements = announcements.filter((a) =>
    instructorCourses.some((c) => c.id === a.courseId)
  )

  const filteredAnnouncements =
    selectedCourse === "all"
      ? instructorAnnouncements
      : instructorAnnouncements.filter((a) => a.courseId === selectedCourse)

  const handleCreate = () => {
    console.log("Creating announcement:", formData)
    setIsCreateOpen(false)
    setFormData({
      title: "",
      content: "",
      courseId: instructorCourses[0]?.id ?? "",
      isPinned: false,
    })
  }

  const stats = {
    total: instructorAnnouncements.length,
    pinned: instructorAnnouncements.filter((a) => a.isPinned).length,
    thisWeek: 2,
    views: 1247,
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Announcements"
        description="Create and manage course announcements"
        breadcrumbs={[
          { label: "Dashboard", href: "/instructor" },
          { label: "Announcements" },
        ]}
        actions={
          <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
            <DialogTrigger asChild>
              <Button>
                <PlusCircleIcon className="mr-2 size-4" />
                New Announcement
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-lg">
              <DialogHeader>
                <DialogTitle>Create Announcement</DialogTitle>
                <DialogDescription>
                  Share important updates with your students.
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="course">Course</Label>
                  <Select
                    value={formData.courseId}
                    onValueChange={(v) =>
                      setFormData((p) => ({ ...p, courseId: v }))
                    }
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Select course" />
                    </SelectTrigger>
                    <SelectContent>
                      {instructorCourses.map((c) => (
                        <SelectItem key={c.id} value={c.id}>
                          {c.code} - {c.title}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="title">Title</Label>
                  <Input
                    id="title"
                    placeholder="Announcement title"
                    value={formData.title}
                    onChange={(e) =>
                      setFormData((p) => ({ ...p, title: e.target.value }))
                    }
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="content">Content</Label>
                  <Textarea
                    id="content"
                    placeholder="Write your announcement..."
                    value={formData.content}
                    onChange={(e) =>
                      setFormData((p) => ({ ...p, content: e.target.value }))
                    }
                    rows={5}
                  />
                </div>
                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="pinned"
                    checked={formData.isPinned}
                    onCheckedChange={(c) =>
                      setFormData((p) => ({ ...p, isPinned: !!c }))
                    }
                  />
                  <Label htmlFor="pinned" className="text-sm font-normal">
                    Pin this announcement
                  </Label>
                </div>
              </div>
              <DialogFooter>
                <Button
                  variant="outline"
                  onClick={() => setIsCreateOpen(false)}
                >
                  Cancel
                </Button>
                <Button
                  onClick={handleCreate}
                  disabled={!formData.title || !formData.content}
                >
                  <SendIcon className="mr-2 size-4" />
                  Publish
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        }
      />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="flex items-center gap-2 text-sm font-medium">
              <MegaphoneIcon className="size-4" />
              Total
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.total}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Pinned</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.pinned}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">This Week</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.thisWeek}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Total Views</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.views}</p>
          </CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <Select value={selectedCourse} onValueChange={setSelectedCourse}>
          <SelectTrigger className="w-64">
            <SelectValue placeholder="Filter by course" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Courses</SelectItem>
            {instructorCourses.map((c) => (
              <SelectItem key={c.id} value={c.id}>
                {c.code} - {c.title}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <Tabs defaultValue="active">
        <TabsList>
          <TabsTrigger value="active">
            Active ({filteredAnnouncements.length})
          </TabsTrigger>
          <TabsTrigger value="archived">Archived (0)</TabsTrigger>
        </TabsList>

        <TabsContent value="active" className="mt-6 space-y-4">
          {filteredAnnouncements.map((announcement) => {
            const course = courses.find((c) => c.id === announcement.courseId)
            return (
              <Card
                key={announcement.id}
                className={cn(
                  announcement.isPinned && "border-primary/50 bg-primary/5"
                )}
              >
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        {announcement.isPinned && (
                          <Badge variant="default">Pinned</Badge>
                        )}
                        <Badge variant="outline">{course?.code}</Badge>
                      </div>
                      <CardTitle>{announcement.title}</CardTitle>
                      <CardDescription>
                        Posted on {announcement.createdAt}
                      </CardDescription>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button size="icon-sm" variant="ghost">
                        <PencilIcon className="size-4" />
                      </Button>
                      <Button
                        size="icon-sm"
                        variant="ghost"
                        className="text-destructive"
                      >
                        <Trash2Icon className="size-4" />
                      </Button>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">
                    {announcement.content}
                  </p>
                </CardContent>
              </Card>
            )
          })}
          {filteredAnnouncements.length === 0 && (
            <Card>
              <CardContent className="py-12 text-center text-muted-foreground">
                No announcements yet. Create your first announcement!
              </CardContent>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="archived" className="mt-6">
          <Card>
            <CardContent className="py-12 text-center text-muted-foreground">
              No archived announcements.
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}
