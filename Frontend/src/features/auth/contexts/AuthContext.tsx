import { createContext } from "react";
import type { IAuthContext } from "./auth.t";

export const AuthContext = createContext<IAuthContext | null>(null);
