import * as React from "react";
import type { User, UserRole } from "@/types";
import {
  api,
  type LoginRequest,
  type AuthResponse,
  mapAuthUser,
  getToken,
  setToken,
  clearToken,
  getStoredUser,
  setStoredUser,
  clearStoredUser,
  ApiError,
} from "@/lib/api-client";

type AuthContextType = {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string, _role: UserRole) => Promise<boolean>;
  logout: () => void;
  switchRole: (role: UserRole) => void;
  loginError: string | null;
};

const AuthContext = React.createContext<AuthContextType | undefined>(undefined);

function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = React.useState<User | null>(() => {
    return getToken() ? getStoredUser() : null;
  });
  const [loginError, setLoginError] = React.useState<string | null>(null);

  const login = React.useCallback(async (email: string, password: string, _role: UserRole): Promise<boolean> => {
    setLoginError(null);
    try {
      const payload: LoginRequest = { email, password };
      const res = await api.post<AuthResponse>("/api/auth/login", payload, { skipAuth: true });
      setToken(res.token);
      const mapped = mapAuthUser(res);
      setUser(mapped);
      setStoredUser(mapped);
      return true;
    } catch (err: unknown) {
      if (err instanceof ApiError) {
        setLoginError(err.message);
      } else if (err instanceof Error) {
        setLoginError(err.message);
      } else {
        setLoginError("Unable to sign in. Please try again.");
      }
      return false;
    }
  }, []);

  const logout = React.useCallback(() => {
    setUser(null);
    clearToken();
    clearStoredUser();
  }, []);

  // switchRole is kept for API compatibility but is a no-op in real auth
  const switchRole = React.useCallback((_role: UserRole) => {
    // no-op: role is determined by the backend
  }, []);

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, logout, switchRole, loginError }}>
      {children}
    </AuthContext.Provider>
  );
}

function useAuth() {
  const context = React.useContext(AuthContext);
  if (context === undefined) throw new Error("useAuth must be used within an AuthProvider");
  return context;
}

export { AuthProvider, useAuth };
