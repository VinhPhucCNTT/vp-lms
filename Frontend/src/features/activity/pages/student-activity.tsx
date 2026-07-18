import * as React from "react";
import { Link } from "react-router-dom";
import {
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  useReactTable,
  type ColumnDef,
} from "@tanstack/react-table";
import { SearchIcon, ClockIcon, CheckCircleIcon, AlertCircleIcon, ListTodoIcon } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { PageHeader } from "@/shared/components/page-header";
import { getStudentActivities } from "@/shared/data/courses";
import { students } from "@/shared/data/users";
import { useAuth } from "@/features/auth/auth-context";
import { cn } from "@/lib/utils";
import type { StudentActivity, ActivityType, ActivityStatus } from "@/types";

const activityTypeLabels: Record<ActivityType, string> = {
  lesson: "Lesson",
  assignment: "Assignment",
  assessment: "Assessment",
  "coding-problem": "Coding Problem",
};

const statusColors: Record<ActivityStatus, string> = {
  pending: "bg-warning/20 text-warning",
  completed: "bg-success/20 text-success",
  overdue: "bg-destructive/20 text-destructive",
};

const typeColors: Record<ActivityType, string> = {
  lesson: "bg-muted text-muted-foreground",
  assignment: "bg-secondary text-secondary-foreground",
  assessment: "bg-info/20 text-info",
  "coding-problem": "bg-accent text-accent-foreground",
};

export function StudentActivity() {
  const { user } = useAuth();
  const currentUser = students.find((s) => s.id === user?.id) ?? students[0];
  // Stable ref required: a new array on every render triggers infinite TanStack Table re-renders
  const studentActivities = React.useMemo(
    () => getStudentActivities(currentUser.enrolledCourses),
    [currentUser.enrolledCourses]
  );

  const [search, setSearch] = React.useState("");
  const [typeFilter, setTypeFilter] = React.useState<string>("all");
  const [statusFilter, setStatusFilter] = React.useState<string>("all");
  const [dueFilter, setDueFilter] = React.useState<string>("all");

  const now = new Date();
  const startOfWeek = new Date(now);
  startOfWeek.setDate(now.getDate() - now.getDay());
  const endOfWeek = new Date(startOfWeek);
  endOfWeek.setDate(startOfWeek.getDate() + 6);

  const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
  const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);

  const filteredActivities = React.useMemo(() => {
    return studentActivities.filter((activity) => {
      const matchesSearch =
        activity.title.toLowerCase().includes(search.toLowerCase()) ||
        activity.course.code.toLowerCase().includes(search.toLowerCase()) ||
        activity.course.title.toLowerCase().includes(search.toLowerCase());

      const matchesType = typeFilter === "all" || activity.type === typeFilter;
      const matchesStatus = statusFilter === "all" || activity.status === statusFilter;
      let matchesDue = true;
      const dueDate = new Date(activity.dueDate);
      if (dueFilter === "this-week") {
        matchesDue = dueDate >= startOfWeek && dueDate <= endOfWeek;
      } else if (dueFilter === "this-month") {
        matchesDue = dueDate >= startOfMonth && dueDate <= endOfMonth;
      } else if (dueFilter === "overdue") {
        matchesDue = activity.status === "overdue";
      }

      return matchesSearch && matchesType && matchesStatus && matchesDue;
    });
  }, [studentActivities, search, typeFilter, statusFilter, dueFilter]);

  const stats = React.useMemo(() => {
    const total = filteredActivities.length;
    const completed = filteredActivities.filter((a) => a.status === "completed").length;
    const pending = filteredActivities.filter((a) => a.status === "pending").length;
    const overdue = filteredActivities.filter((a) => a.status === "overdue").length;
    return { total, completed, pending, overdue };
  }, [filteredActivities]);

  const columns: ColumnDef<StudentActivity>[] = React.useMemo(() => [
    {
      accessorKey: "title",
      header: "Activity",
      cell: ({ row }) => {
        const activity = row.original;
        const href = activity.type === "coding-problem"
          ? `/student/courses/${activity.course.id}/problems/${activity.refId}`
          : activity.type === "assessment"
          ? `/student/courses/${activity.course.id}/assessments/${activity.refId}`
          : `/student/courses/${activity.course.id}`;

        return (
          <Link to={href} className="hover:underline font-medium">
            {activity.title}
          </Link>
        );
      },
    },
    {
      accessorKey: "course",
      header: "Course",
      cell: ({ row }) => {
        const course = row.original.course;
        return (
          <div className="flex flex-col">
            <span className="font-medium">{course.code}</span>
            <span className="text-xs text-muted-foreground line-clamp-1">{course.title}</span>
          </div>
        );
      },
    },
    {
      accessorKey: "type",
      header: "Type",
      cell: ({ row }) => {
        const type = row.original.type;
        return (
          <Badge className={cn("text-xs", typeColors[type])}>
            {activityTypeLabels[type]}
          </Badge>
        );
      },
    },
    {
      accessorKey: "dueDate",
      header: "Due Date",
      cell: ({ row }) => {
        if (row.original.type === "lesson")
          return;
        const dueDate = new Date(row.original.dueDate);
        const isOverdue = row.original.status === "overdue";
        return (
          <span className={cn("text-sm", isOverdue && "text-destructive font-medium")}>
            {dueDate.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" })}
          </span>
        );
      },
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => {
        const status = row.original.status;
        const statusLabels: Record<ActivityStatus, string> = {
          pending: "Pending",
          completed: "Completed",
          overdue: "Overdue",
        };
        return (
          <Badge className={cn("text-xs", statusColors[status])}>
            {statusLabels[status]}
          </Badge>
        );
      },
    },
    {
      id: "actions",
      cell: ({ row }) => {
        const activity = row.original;
        const href = activity.type === "coding-problem"
          ? `/student/courses/${activity.course.id}/problems/${activity.refId}`
          : activity.type === "assessment"
          ? `/student/courses/${activity.course.id}/assessments/${activity.refId}`
          : `/student/courses/${activity.course.id}`;

        return (
          <Button size="sm" variant="outline" asChild>
            <Link to={href}>
              {activity.status === "completed" ? "Review" : "Start"}
            </Link>
          </Button>
        );
      },
    },
  ], []);

  const table = useReactTable({
    data: filteredActivities,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: {
      pagination: {
        pageSize: 10,
      },
    },
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Activities"
        description="Track and manage all your course activities"
        breadcrumbs={[
          { label: "Dashboard", href: "/student" },
          { label: "Activities" },
        ]}
      />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <ListTodoIcon className="size-4" />
              Total
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.total}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <CheckCircleIcon className="size-4 text-success" />
              Completed
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.completed}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <ClockIcon className="size-4 text-warning" />
              Pending
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.pending}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <AlertCircleIcon className="size-4 text-destructive" />
              Overdue
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-2xl font-bold">{stats.overdue}</p>
          </CardContent>
        </Card>
      </div>

      <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
        <div className="relative flex-1 max-w-sm">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input
            placeholder="Search activities..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
          />
        </div>

        <div className="flex flex-wrap gap-2">
          <Select value={typeFilter} onValueChange={setTypeFilter}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Type" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Types</SelectItem>
              <SelectItem value="lesson">Lesson</SelectItem>
              <SelectItem value="assignment">Assignment</SelectItem>
              <SelectItem value="assessment">Assessment</SelectItem>
              <SelectItem value="coding-problem">Coding Problem</SelectItem>
            </SelectContent>
          </Select>

          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="pending">Pending</SelectItem>
              <SelectItem value="completed">Completed</SelectItem>
              <SelectItem value="overdue">Overdue</SelectItem>
            </SelectContent>
          </Select>

          <Select value={dueFilter} onValueChange={setDueFilter}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Due" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Dates</SelectItem>
              <SelectItem value="this-week">This Week</SelectItem>
              <SelectItem value="this-month">This Month</SelectItem>
              <SelectItem value="overdue">Overdue</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext()
                        )}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {table.getRowModel().rows?.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id}>
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext()
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center"
                >
                  No activities found.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      <div className="flex items-center justify-between">
        <div className="text-sm text-muted-foreground">
          Showing {table.getState().pagination.pageIndex * table.getState().pagination.pageSize + 1} to{" "}
          {Math.min(
            (table.getState().pagination.pageIndex + 1) * table.getState().pagination.pageSize,
            filteredActivities.length
          )}{" "}
          of {filteredActivities.length} activities
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => table.previousPage()}
            disabled={!table.getCanPreviousPage()}
          >
            Previous
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => table.nextPage()}
            disabled={!table.getCanNextPage()}
          >
            Next
          </Button>
        </div>
      </div>
    </div>
  );
}
