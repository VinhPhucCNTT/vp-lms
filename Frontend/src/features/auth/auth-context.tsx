import * as React from "react"
import type { User, UserRole } from "@/types"
import { students, instructors, admins } from "@/shared/data/users"

type AuthContextType = {
  user: User | null
  isAuthenticated: boolean
  login: (email: string, password: string, role: UserRole) => Promise<boolean>
  logout: () => void
  switchRole: (role: UserRole) => void
}

const AuthContext = React.createContext<AuthContextType | undefined>(undefined)

function getStoredUser(): User | null {
  const stored = localStorage.getItem("lms-user")
  if (stored) {
    try {
      return JSON.parse(stored)
    } catch {
      return null
    }
  }
  return null
}

function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = React.useState<User | null>(getStoredUser)

  const login = React.useCallback(
    async (
      _email: string,
      _password: string,
      role: UserRole
    ): Promise<boolean> => {
      let foundUser: User | null = null
      if (role === "student") foundUser = students[0]
      else if (role === "instructor") foundUser = instructors[0]
      else if (role === "admin") foundUser = admins[0]
      if (foundUser) {
        setUser(foundUser)
        localStorage.setItem("lms-user", JSON.stringify(foundUser))
        return true
      }
      return false
    },
    []
  )

  const logout = React.useCallback(() => {
    setUser(null)
    localStorage.removeItem("lms-user")
  }, [])

  const switchRole = React.useCallback((role: UserRole) => {
    let newUser: User | null = null
    if (role === "student") newUser = students[0]
    else if (role === "instructor") newUser = instructors[0]
    else if (role === "admin") newUser = admins[0]
    if (newUser) {
      setUser(newUser)
      localStorage.setItem("lms-user", JSON.stringify(newUser))
    }
  }, [])

  return (
    <AuthContext.Provider
      value={{ user, isAuthenticated: !!user, login, logout, switchRole }}
    >
      {children}
    </AuthContext.Provider>
  )
}

function useAuth() {
  const context = React.useContext(AuthContext)
  if (context === undefined)
    throw new Error("useAuth must be used within an AuthProvider")
  return context
}

export { AuthProvider, useAuth }
