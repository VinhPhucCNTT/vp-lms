import * as React from "react"
import { Navigate, Outlet } from "react-router-dom"
import { useAuth } from "@/features/auth/auth-context"
import type { UserRole } from "@/types"

function RoleGuard({ allowedRoles }: { allowedRoles: UserRole[] }) {
  const { user, isAuthenticated } = useAuth()
  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (user && !allowedRoles.includes(user.role)) {
    const redirectPath =
      user.role === "student"
        ? "/student"
        : user.role === "instructor"
          ? "/instructor"
          : "/admin"
    return <Navigate to={redirectPath} replace />
  }
  return <Outlet />
}

function StudentRoute() {
  return <RoleGuard allowedRoles={["student"]} />
}
function InstructorRoute() {
  return <RoleGuard allowedRoles={["instructor"]} />
}
function AdminRoute() {
  return <RoleGuard allowedRoles={["admin"]} />
}
function GuestRoute() {
  const { isAuthenticated } = useAuth()
  if (isAuthenticated) return <Navigate to="/" replace />
  return <Outlet />
}

export { StudentRoute, InstructorRoute, AdminRoute, GuestRoute }
