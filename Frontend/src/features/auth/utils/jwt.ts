import { jwtDecode } from "jwt-decode";

import type { IUser } from "../contexts/auth.t";

const CLAIM_NAME_ID =
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
const CLAIM_ROLE =
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export const parseJwt = (token: string): IUser => {
    const payload = jwtDecode<Record<string, unknown>>(token);

    const email =
        typeof payload.email === "string"
            ? payload.email
            : typeof payload["Email"] === "string"
                ? (payload["Email"] as string)
                : "";

    const userIdRaw =
        (typeof payload.sub === "string" && payload.sub) ||
        (typeof payload[CLAIM_NAME_ID] === "string" ? payload[CLAIM_NAME_ID] : "");
    const userId = typeof userIdRaw === "string" ? userIdRaw : "";

    const roles: string[] = [];

    const pushRole = (v: unknown) => {
        if (typeof v === "string") roles.push(v);
        else if (Array.isArray(v)) v.forEach(pushRole);
    };

    // TODO: ????
    pushRole(payload.role);
    pushRole(payload[CLAIM_ROLE]);
    pushRole((payload as { Role?: unknown }).Role);

    const unique = [...new Set(roles)];

    return {
        email,
        roles: unique,
        userId,
    };
};
