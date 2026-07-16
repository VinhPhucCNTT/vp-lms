import * as React from "react";
import { SearchIcon, FilterIcon, ClockIcon, UserIcon, ShieldCheckIcon, AlertTriangleIcon, InfoIcon, DownloadIcon } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import { PageHeader } from "@/shared/components/page-header";
import { cn } from "@/lib/utils";

type AuditSeverity = "info" | "warning" | "critical";
type AuditAction = "login" | "logout" | "create" | "update" | "delete" | "view" | "export";

interface AuditLogEntry {
  id: string;
  timestamp: string;
  userId: string;
  userName: string;
  userRole: string;
  action: AuditAction;
  resource: string;
  resourceId: string;
  details: string;
  ipAddress: string;
  severity: AuditSeverity;
  userAgent: string;
}

const auditLogs: AuditLogEntry[] = [
  { id: "log-001", timestamp: "2026-01-17 14:32:15", userId: "ins-001", userName: "Dr. Robert Smith", userRole: "instructor", action: "update", resource: "course", resourceId: "cs-101", details: "Updated course description for CS 101", ipAddress: "192.168.1.45", severity: "info", userAgent: "Chrome/120.0" },
  { id: "log-002", timestamp: "2026-01-17 14:28:33", userId: "stu-001", userName: "Alex Chen", userRole: "student", action: "create", resource: "submission", resourceId: "sub-009", details: "Submitted assignment: Algorithm Analysis Practice", ipAddress: "10.0.0.23", severity: "info", userAgent: "Firefox/121.0" },
  { id: "log-003", timestamp: "2026-01-17 14:25:10", userId: "adm-001", userName: "System Administrator", userRole: "admin", action: "create", resource: "user", resourceId: "stu-009", details: "Created new student account: john.doe@university.edu", ipAddress: "192.168.1.1", severity: "info", userAgent: "Chrome/120.0" },
  { id: "log-004", timestamp: "2026-01-17 14:20:45", userId: "ins-002", userName: "Prof. Emily Johnson", userRole: "instructor", action: "create", resource: "announcement", resourceId: "ann-004", details: "Created announcement for CS 201", ipAddress: "192.168.1.67", severity: "info", userAgent: "Safari/17.2" },
  { id: "log-005", timestamp: "2026-01-17 14:15:22", userId: "stu-002", userName: "Sarah Johnson", userRole: "student", action: "view", resource: "submission", resourceId: "sub-002", details: "Viewed submission results for Two Sum problem", ipAddress: "10.0.0.45", severity: "info", userAgent: "Chrome/120.0" },
  { id: "log-006", timestamp: "2026-01-17 14:10:18", userId: "unknown", userName: "Unknown User", userRole: "unknown", action: "login", resource: "auth", resourceId: "attempt-001", details: "Failed login attempt for email: admin@university.edu", ipAddress: "203.0.113.42", severity: "warning", userAgent: "curl/7.68.0" },
  { id: "log-007", timestamp: "2026-01-17 14:05:55", userId: "admin-001", userName: "System Administrator", userRole: "admin", action: "delete", resource: "submission", resourceId: "sub-010", details: "Deleted duplicate submission", ipAddress: "192.168.1.1", severity: "warning", userAgent: "Chrome/120.0" },
  { id: "log-008", timestamp: "2026-01-17 14:00:30", userId: "ins-003", userName: "Dr. David Williams", userRole: "instructor", action: "update", resource: "assignment", resourceId: "asg-003", details: "Changed due date for Database Design Project", ipAddress: "192.168.1.89", severity: "info", userAgent: "Firefox/121.0" },
  { id: "log-009", timestamp: "2026-01-17 13:55:12", userId: "unknown", userName: "Unknown User", userRole: "unknown", action: "login", resource: "auth", resourceId: "attempt-002", details: "Multiple failed login attempts detected (5+ attempts)", ipAddress: "198.51.100.23", severity: "critical", userAgent: "python-requests/2.28.0" },
  { id: "log-010", timestamp: "2026-01-17 13:50:00", userId: "system", userName: "System", userRole: "system", action: "create", resource: "backup", resourceId: "backup-2026-01-17", details: "Automated daily backup completed successfully", ipAddress: "127.0.0.1", severity: "info", userAgent: "System/Cron" },
  { id: "log-011", timestamp: "2026-01-17 13:45:25", userId: "stu-003", userName: "Michael Brown", userRole: "student", action: "view", resource: "course", resourceId: "cs-301", details: "Accessed course materials for CS 301", ipAddress: "10.0.0.67", severity: "info", userAgent: "Mobile Safari/17.2" },
  { id: "log-012", timestamp: "2026-01-17 13:40:18", userId: "adm-001", userName: "System Administrator", userRole: "admin", action: "export", resource: "report", resourceId: "report-001", details: "Exported user activity report (CSV)", ipAddress: "192.168.1.1", severity: "info", userAgent: "Chrome/120.0" },
];

const severityColors: Record<AuditSeverity, string> = {
  info: "bg-info/20 text-info",
  warning: "bg-warning/20 text-warning-foreground",
  critical: "bg-destructive/20 text-destructive",
};

const actionColors: Record<AuditAction, string> = {
  login: "bg-muted text-muted-foreground",
  logout: "bg-muted text-muted-foreground",
  create: "bg-success/20 text-success",
  update: "bg-info/20 text-info",
  delete: "bg-destructive/20 text-destructive",
  view: "bg-secondary text-secondary-foreground",
  export: "bg-warning/20 text-warning-foreground",
};

export function AdminAuditLog() {
  const [search, setSearch] = React.useState("");
  const [actionFilter, setActionFilter] = React.useState("all");
  const [severityFilter, setSeverityFilter] = React.useState("all");

  const filteredLogs = auditLogs.filter((log) => {
    const matchesSearch = log.userName.toLowerCase().includes(search.toLowerCase()) || log.resource.toLowerCase().includes(search.toLowerCase()) || log.details.toLowerCase().includes(search.toLowerCase());
    const matchesAction = actionFilter === "all" || log.action === actionFilter;
    const matchesSeverity = severityFilter === "all" || log.severity === severityFilter;
    return matchesSearch && matchesAction && matchesSeverity;
  });

  const stats = {
    total: auditLogs.length,
    info: auditLogs.filter((l) => l.severity === "info").length,
    warning: auditLogs.filter((l) => l.severity === "warning").length,
    critical: auditLogs.filter((l) => l.severity === "critical").length,
  };

  return (
    <div className="space-y-6">
      <PageHeader title="Audit Log" description="System activity and security monitoring" breadcrumbs={[{ label: "Dashboard", href: "/admin" }, { label: "Audit Log" }]} actions={<Button variant="outline"><DownloadIcon className="size-4 mr-2" />Export Logs</Button>} />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><InfoIcon className="size-4" />Total Events</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.total}</p><p className="text-xs text-muted-foreground">Last 24 hours</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium">Info</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.info}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><AlertTriangleIcon className="size-4 text-warning" />Warnings</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.warning}</p></CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-2"><CardTitle className="text-sm font-medium flex items-center gap-2"><ShieldCheckIcon className="size-4 text-destructive" />Critical</CardTitle></CardHeader>
          <CardContent><p className="text-2xl font-bold">{stats.critical}</p></CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-md">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground" />
          <Input placeholder="Search logs..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Select value={actionFilter} onValueChange={setActionFilter}>
          <SelectTrigger className="w-40"><SelectValue placeholder="Action" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Actions</SelectItem>
            <SelectItem value="login">Login</SelectItem>
            <SelectItem value="create">Create</SelectItem>
            <SelectItem value="update">Update</SelectItem>
            <SelectItem value="delete">Delete</SelectItem>
            <SelectItem value="view">View</SelectItem>
            <SelectItem value="export">Export</SelectItem>
          </SelectContent>
        </Select>
        <Select value={severityFilter} onValueChange={setSeverityFilter}>
          <SelectTrigger className="w-40"><SelectValue placeholder="Severity" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Severities</SelectItem>
            <SelectItem value="info">Info</SelectItem>
            <SelectItem value="warning">Warning</SelectItem>
            <SelectItem value="critical">Critical</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Card>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-32">Timestamp</TableHead>
              <TableHead>User</TableHead>
              <TableHead>Action</TableHead>
              <TableHead>Resource</TableHead>
              <TableHead>Details</TableHead>
              <TableHead>IP Address</TableHead>
              <TableHead>Severity</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredLogs.map((log) => (
              <TableRow key={log.id} className={cn(log.severity === "critical" && "bg-destructive/5", log.severity === "warning" && "bg-warning/5")}>
                <TableCell className="font-mono text-xs text-muted-foreground">{log.timestamp}</TableCell>
                <TableCell>
                  <div className="flex items-center gap-2">
                    <Avatar size="sm"><AvatarFallback>{log.userName.split(" ").map((n) => n[0]).join("").slice(0, 2)}</AvatarFallback></Avatar>
                    <div>
                      <p className="font-medium text-sm">{log.userName}</p>
                      <p className="text-xs text-muted-foreground capitalize">{log.userRole}</p>
                    </div>
                  </div>
                </TableCell>
                <TableCell><Badge variant="outline" className={cn(actionColors[log.action], "capitalize")}>{log.action}</Badge></TableCell>
                <TableCell>
                  <div>
                    <p className="text-sm font-medium capitalize">{log.resource}</p>
                    <p className="text-xs text-muted-foreground">{log.resourceId}</p>
                  </div>
                </TableCell>
                <TableCell className="max-w-xs"><p className="text-sm truncate">{log.details}</p></TableCell>
                <TableCell className="font-mono text-xs">{log.ipAddress}</TableCell>
                <TableCell><Badge className={cn(severityColors[log.severity], "capitalize")}>{log.severity}</Badge></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </div>
  );
}
