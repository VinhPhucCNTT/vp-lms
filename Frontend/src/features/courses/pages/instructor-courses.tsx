import * as React from "react";
import { PlusIcon, SearchIcon, UsersIcon, } from "lucide-react";
import { Link, useSearchParams } from "react-router-dom";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { PageHeader } from "@/shared/components/page-header";
import { useApi } from "@/lib/use-api";
import { LoadingState, ErrorState, EmptyState } from "@/shared/components/api-states";
import { courseApi, CourseSetResponse, InstructorCourseResponse } from "@/features/courses/api/course-api";
import { PaginatedResponse } from "@/shared/types";
import { CourseCreateDialog } from "../components/course-create-dialog";

export function InstructorCourses() {
    const { data: courses, loading, error, reload } = useApi<PaginatedResponse<InstructorCourseResponse>>(() => courseApi.getInstructorCourses());
    const [search, setSearch] = React.useState("");
    const [showCreate, setShowCreate] = React.useState(false);
    // const [createError, setCreateError] = React.useState<string | null>(null);
    // const [creating, setCreating] = React.useState(false);
    const [searchParams, setSearchParams] = useSearchParams();

    React.useEffect(() => {
        setShowCreate(searchParams.get("action") === "create");
    }, [searchParams]);

    const filtered = React.useMemo(() => {
        if (!courses) return [];
        let list = courses.data;
        if (search.trim()) {
            const q = search.toLowerCase();
            list = list.filter((c) => c.info.title.toLowerCase().includes(q));
        }
        return list;
    }, [courses, search]);

    const published = filtered.filter((c) => c.isPublished === true);
    const unpublished = filtered.filter((c) => c.isPublished === false);

    // async function handleCreate() {
    //     setCreating(true);
    //     setSearchParams({ action: "create" });
    //     setCreateError(null);
    //     try {
    //         await courseApi.createCourse(form);
    //         setForm({ title: "", description: null, enrollmentOpen: true, backgroundFile: null });
    //         setShowCreate(false);
    //         reload();
    //     } catch (err: unknown) {
    //         setCreateError(err instanceof Error ? err.message : "Unable to create course.");
    //     } finally {
    //         setCreating(false);
    //     }
    // }

    async function onCreating() {
        setSearchParams({ action: "create" });
    }

    async function onCreated(response: CourseSetResponse) {
        setSearchParams({});
        console.log(response);
        reload();
    }

    if (loading) return <LoadingState label="Loading your courses..." />;
    if (error) return <ErrorState message={error} onRetry={reload} />;

    function CourseRow({ course }: { course: InstructorCourseResponse }) {
        return (
            <Card className="group">
                <CardHeader className="pb-2">
                    <div className="flex items-start justify-between gap-4">
                        <div className="space-y-1 min-w-0">
                            <div className="flex items-center gap-2">
                                {/* <Badge variant="outline" className="text-xs shrink-0">{course.code}</Badge> */}
                                {/* <Badge variant={course.status === "published" ? "success" : "secondary"} className="text-xs shrink-0">{course.status}</Badge> */}
                            </div>
                            <h3 className="font-semibold leading-tight">{course.info.title}</h3>
                        </div>
                    </div>
                    <p className="text-sm text-muted-foreground line-clamp-2">{course.info.description}</p>
                </CardHeader>
                <CardContent className="pb-2">
                    <div className="flex items-center gap-4 text-xs text-muted-foreground">
                        <div className="flex items-center gap-1">
                            <UsersIcon className="size-3" />
                            <span>{course.info.enrollmentCount} students</span>
                        </div>
                        {/* <div className="flex items-center gap-1"> */}
                        {/*     <LayersIcon className="size-3" /> */}
                        {/*     <span>{course.credits ?? "—"} credits</span> */}
                        {/* </div> */}
                        {/* <div className="flex items-center gap-1"> */}
                        {/*     <BookOpenIcon className="size-3" /> */}
                        {/*     <span>{course.department ?? "—"}</span> */}
                        {/* </div> */}
                    </div>
                </CardContent>
                <CardFooter className="pt-2 border-t">
                    <div className="flex items-center gap-2 ml-auto">
                        {/* {course.status === "draft" && ( */}
                        {/*     <Button size="sm" variant="default">Publish</Button> */}
                        {/* )} */}
                        {/* <Button size="sm" variant="outline">Edit</Button> */}
                        <Button size="sm" variant="secondary" asChild>
                            <Link to={`/instructor/courses/${course.info.id}`}>View Course</Link>
                        </Button>
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
                    <CourseCreateDialog
                        onCreating={() => onCreating()}
                        onCreated={(response) => onCreated(response)}
                    />
                }
            />

            {/* {showCreate && ( */}
            {/*     <Card> */}
            {/*         <CardHeader><h2 className="font-semibold">Create Course</h2></CardHeader> */}
            {/*         <CardContent> */}
            {/*             <form onSubmit={handleCreate} className="grid gap-4 sm:grid-cols-2"> */}
            {/*                 <Input placeholder="Course title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required /> */}
            {/*                 <Textarea className="sm:col-span-2" placeholder="Description (optional)" value={form.description ?? ""} onChange={(e) => setForm({ ...form, description: e.target.value })} /> */}
            {/*                 {createError && <p className="sm:col-span-2 text-sm text-destructive">{createError}</p>} */}
            {/*                 <div className="sm:col-span-2 flex justify-end gap-2"> */}
            {/*                     <Button type="button" variant="outline" onClick={() => setShowCreate(false)}>Cancel</Button> */}
            {/*                     <Button type="submit" disabled={creating}>{creating ? "Creating..." : "Create Course"}</Button> */}
            {/*                 </div> */}
            {/*             </form> */}
            {/*         </CardContent> */}
            {/*     </Card> */}
            {/* )} */}

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
                    <TabsTrigger value="all">All ({filtered.length})</TabsTrigger>
                    <TabsTrigger value="published">Published ({published.length})</TabsTrigger>
                    <TabsTrigger value="draft">Unpublished ({unpublished.length})</TabsTrigger>
                </TabsList>

                <TabsContent value="all" className="mt-6">
                    {filtered.length === 0 ? (
                        <EmptyState message="No courses found." />
                    ) : (
                        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                            {filtered.map((c) => <CourseRow key={c.info.id} course={c} />)}
                        </div>
                    )}
                </TabsContent>

                <TabsContent value="published" className="mt-6">
                    {published.length === 0 ? (
                        <EmptyState message="No published courses." />
                    ) : (
                        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                            {published.map((c) => <CourseRow key={c.info.id} course={c} />)}
                        </div>
                    )}
                </TabsContent>

                <TabsContent value="unpublished" className="mt-6">
                    {unpublished.length === 0 ? (
                        <EmptyState message="No unpublished courses." />
                    ) : (
                        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                            {unpublished.map((c) => <CourseRow key={c.info.id} course={c} />)}
                        </div>
                    )}
                </TabsContent>
            </Tabs>
        </div>
    );
}
