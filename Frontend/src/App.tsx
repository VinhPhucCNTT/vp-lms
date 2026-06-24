import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { ThemeProvider } from "@/components/theme-provider";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AuthProvider, useAuth } from "@/features/auth/auth-context";
import { GuestRoute, StudentRoute, InstructorRoute, AdminRoute } from "@/router";

import { LoginPage } from "@/features/auth/login-page";

import { StudentLayout } from "@/shared/layouts/student-layout";
import { InstructorLayout } from "@/shared/layouts/instructor-layout";
import { AdminLayout } from "@/shared/layouts/admin-layout";

import { StudentDashboard } from "@/features/dashboard/pages/student-dashboard";
import { InstructorDashboard } from "@/features/dashboard/pages/instructor-dashboard";
import { AdminDashboard } from "@/features/admin/pages/admin-dashboard";

import { StudentCourses } from "@/features/courses/pages/student-courses";
import { CourseWorkspace } from "@/features/courses/pages/course-workspace";
import { InstructorSubmissions } from "@/features/courses/pages/instructor-submissions";
import { InstructorGradebook } from "@/features/courses/pages/instructor-gradebook";
import { InstructorAnnouncements } from "@/features/courses/pages/instructor-announcements";

import { ProblemList } from "@/features/online-judge/pages/problem-list";
import { ProblemSolution } from "@/features/online-judge/pages/problem-solution";

import { StudentAssignments } from "@/features/assignments/pages/student-assignments";
import { StudentAssessments } from "@/features/assessments/pages/student-assessments";

import { UserManagement } from "@/features/admin/pages/user-management";
import { AdminReports } from "@/features/admin/pages/admin-reports";
import { AdminAuditLog } from "@/features/admin/pages/admin-audit-log";

function RootRedirect() {
  const { user, isAuthenticated } = useAuth();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  const redirectPath = user?.role === "student" ? "/student" : user?.role === "instructor" ? "/instructor" : "/admin";
  return <Navigate to={redirectPath} replace />;
}

function App() {
  return (
    <ThemeProvider>
      <TooltipProvider>
        <BrowserRouter>
          <AuthProvider>
            <Routes>
              <Route path="/" element={<RootRedirect />} />
              <Route element={<GuestRoute />}>
                <Route path="/login" element={<LoginPage />} />
              </Route>
              <Route element={<StudentRoute />}>
                <Route path="/student" element={<StudentLayout />}>
                  <Route index element={<StudentDashboard />} />
                  <Route path="courses" element={<StudentCourses />} />
                  <Route path="courses/:courseId" element={<CourseWorkspace />} />
                  <Route path="assignments" element={<StudentAssignments />} />
                  <Route path="assessments" element={<StudentAssessments />} />
                  <Route path="judge" element={<ProblemList />} />
                  <Route path="judge/:problemId" element={<ProblemSolution />} />
                </Route>
              </Route>
              <Route element={<InstructorRoute />}>
                <Route path="/instructor" element={<InstructorLayout />}>
                  <Route index element={<InstructorDashboard />} />
                  <Route path="courses" element={<InstructorDashboard />} />
                  <Route path="builder" element={<InstructorDashboard />} />
                  <Route path="submissions" element={<InstructorSubmissions />} />
                  <Route path="gradebook" element={<InstructorGradebook />} />
                  <Route path="announcements" element={<InstructorAnnouncements />} />
                </Route>
              </Route>
              <Route element={<AdminRoute />}>
                <Route path="/admin" element={<AdminLayout />}>
                  <Route index element={<AdminDashboard />} />
                  <Route path="users" element={<UserManagement />} />
                  <Route path="courses" element={<AdminDashboard />} />
                  <Route path="reports" element={<AdminReports />} />
                  <Route path="audit" element={<AdminAuditLog />} />
                  <Route path="settings" element={<AdminDashboard />} />
                </Route>
              </Route>
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </AuthProvider>
        </BrowserRouter>
      </TooltipProvider>
    </ThemeProvider>
  );
}

export default App;
