import { useMemo, useState } from "react";

import type { PropsWithChildren } from "react";

import { parseJwt } from "../utils/jwt";
import type { IUser } from "./auth.t";
import { AuthContext } from "./AuthContext";

const TOKEN_KEY = "access_token";

export const AuthProvider = ({ children }: PropsWithChildren) => {
    const [token, setToken] = useState<string | null>(() =>
        localStorage.getItem(TOKEN_KEY)
    );
    const [user, setUser] = useState<IUser | null>(() => {
        const token = localStorage.getItem(TOKEN_KEY);

        return token ? parseJwt(token) : null;
    });

    const login = (newToken: string) => {
        localStorage.setItem(TOKEN_KEY, newToken);

        setToken(newToken);
        setUser(parseJwt(newToken));
    };

    const logout = () => {
        localStorage.removeItem(TOKEN_KEY);

        setToken(null);
        setUser(null);
    };

    const value = useMemo(
        () => ({
            user,
            token,
            isAuthenticated: !!token,
            login,
            logout,
        }),
        [user, token],
    );

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};

