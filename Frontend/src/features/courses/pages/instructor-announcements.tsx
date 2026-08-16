import * as React from "react";
import { PlusCircleIcon, PencilIcon, Trash2Icon, SendIcon, MegaphoneIcon } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PageHeader } from "@/shared/components/page-header";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { useApi } from "@/lib/use-api";
import { instructorApi, type AnnouncementDto } from "@/features/courses/instructor-api";
import { courseApi } from "@/features/courses/course-api";
import type { Course } from "@/types";
import { cn } from "@/lib/utils";

interface AnnouncementFormData {
  title: string;
  content: string;
  courseId: string;
  isPinned: boolean;
}

export function InstructorAnnouncements() {
  const { data: announcements, loading, error, reload } = useApi<AnnouncementDto[]>(() => instructorApi.getAnnouncements());
  const { data: courses } = useApi<Course[]>(() => courseApi.getInstructorCourses());
  const [isCreateOpen, setIsCreateOpen] = React.useState(false);
  const [selectedCourse, setSelectedCourse] = React.useState<string>("all");
  const [creating, setCreating] = React.useState(false);
  const [createError, setCreateError] = React.useState<string | null>(null);
  const [formData, setFormData] = React.useState<AnnouncementFormData>({ title: "", content: "", courseId: "", isPinned: false });

  React.useEffect(() => {
    if (courses && courses.length > 0 && !formData.courseId) {
      setFormData((p) => ({ ...p, courseId: courses[0].id }));
    }
  }, [courses, formData.courseId]);

  const instructorCourses = courses ?? [];
  const filteredAnnouncements = selectedCourse === "all" ? (announcements ?? []) : (announcements ?? []).filter((a) => a.courseId === selectedCourse);

  const handleCreate = async () => {
    setCreating(true);
    setCreateError(null);
    try {
      await instructorApi.createAnnouncement(formData);
      setIsCreateOpen(false);
      setFormData({ title: "", content: "", courseId: instructorCourses[0]?.id ?? "", isPinned: false });
      reload();
    } catch (err: unknown) {
      setCreateError(err instanceof Error ? err.message : "Failed to create announcement.");
    } finally {
      setCreating(false);
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await instructorApi.deleteAnnouncement(id);
      reload();
    } catch (err: unknown) {
      console.error("Failed to delete announcement:", err);
    }
  };

  if (loading) return <LoadingState label="Loading announcements..." />;
  if (error) return <ErrorState message={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <PageHeader title="Announcements" description="Create and manage course announcements" breadcrumbs={[{ label: "Dashboard", href: "/instructor" }, { label: "Announcements" }]} actions={
        <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
          <DialogTrigger asChild><Button><PlusCircleIcon className="size-4 mr-2" />New Announcement</Button></DialogTrigger>
          <DialogContent className="max-w-lg">
            <DialogHeader><DialogTitle>Create Announcement</DialogTitle><DialogDescription>Share important updates with your students.</DialogDescription></DialogHeader>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="course">Course</Label>
                <Select value={formData.courseId} onValueChange={(v) => setFormData((p) => ({ ...p, courseId: v }))}>
                  <SelectTrigger><SelectValue placeholder="Select course" /></SelectTrigger>
                  <SelectContent>
                    {instructorCourses.map((c) => (<SelectItem key={c.id} value={c.id}>{c.code} - {c.title}</SelectItem>))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="title">Title</Label>
                <Input id="title" placeholder="Announcement title" value={formData.title} onChange={(e) => setFormData((p) => ({ ...p, title: e.target.value }))} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="content">Content</Label>
                <Textarea id="content" placeholder="Write your announcement..." value={formData.content} onChange={(e) => setFormData((p) => ({ ...p, content: e.target.value }))} rows={5} />
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox id="pinned" checked={formData.isPinned} onCheckedChange={(c) => setFormData((p) => ({ ...p, isPinned: !!c }))} />
                <Label htmlFor="pinned" className="text-sm font-normal">Pin this announcement</Label>
              </div>
              {createError && <p className="text-sm text-destructive">{createError}</p>}
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsCreateOpen(false)}>Cancel</Button>
              <Button onClick={handleCreate} disabled={!formData.title || !formData.content || creating}><SendIcon className="size-4 mr-2" />{creating ? "Publishing..." : "Publish"}</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      } />

      <div className="grid gap-4 md:grid-cols-4">
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><MegaphoneIcon className="size-4" />Total</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold">{announcements?.length ?? 0}</p></CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Pinned</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold">{announcements?.filter((a) => a.isPinned).length ?? 0}</p></CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium">This Week</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold">—</p></CardContent></Card>
        <Card><CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Total Views</CardTitle></CardHeader><CardContent><p className="text-2xl font-bold">—</p></CardContent></Card>
      </div>

      <div className="flex items-center gap-4">
        <Select value={selectedCourse} onValueChange={setSelectedCourse}>
          <SelectTrigger className="w-64"><SelectValue placeholder="Filter by course" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Courses</SelectItem>
            {instructorCourses.map((c) => (<SelectItem key={c.id} value={c.id}>{c.code} - {c.title}</SelectItem>))}
          </SelectContent>
        </Select>
      </div>

      <Tabs defaultValue="active">
        <TabsList>
          <TabsTrigger value="active">Active ({filteredAnnouncements.length})</TabsTrigger>
          <TabsTrigger value="archived">Archived (0)</TabsTrigger>
        </TabsList>

        <TabsContent value="active" className="mt-6 space-y-4">
          {filteredAnnouncements.length === 0 ? (
            <EmptyState message="No announcements yet. Create your first announcement!" />
          ) : (
            filteredAnnouncements.map((announcement) => (
              <Card key={announcement.id} className={cn(announcement.isPinned && "border-primary/50 bg-primary/5")}>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        {announcement.isPinned && <Badge variant="default">Pinned</Badge>}
                        <Badge variant="outline">{announcement.courseCode ?? "—"}</Badge>
                      </div>
                      <CardTitle>{announcement.title}</CardTitle>
                      <CardDescription>Posted on {announcement.createdAt}</CardDescription>
                    </div>
                    <div className="flex items-center gap-2">
                      <Button size="icon-sm" variant="ghost"><PencilIcon className="size-4" /></Button>
                      <Button size="icon-sm" variant="ghost" className="text-destructive" onClick={() => handleDelete(announcement.id)}><Trash2Icon className="size-4" /></Button>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-sm text-muted-foreground">{announcement.content}</p>
                </CardContent>
              </Card>
            ))
          )}
        </TabsContent>

        <TabsContent value="archived" className="mt-6">
          <EmptyState message="No archived announcements." />
        </TabsContent>
      </Tabs>
    </div>
  );
}
