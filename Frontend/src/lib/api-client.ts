import type { User, UserRole } from "@/types";

const TOKEN_KEY = "lms-token";
const USER_KEY = "lms-user";

export function getBaseUrl(): string {
    return import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5039";
}

export function getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken(): void {
    localStorage.removeItem(TOKEN_KEY);
}

export function getStoredUser(): User | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
        return JSON.parse(raw) as User;
    } catch {
        return null;
    }
}

export function setStoredUser(user: User): void {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearStoredUser(): void {
    localStorage.removeItem(USER_KEY);
}

export class ApiError extends Error {
    constructor(
        public status: number,
        public message: string,
        public body?: unknown,
    ) {
        super(message);
        this.name = "ApiError";
    }
}

type RequestOptions = {
    method?: string;
    body?: unknown;
    headers?: Record<string, string>;
    signal?: AbortSignal;
    skipAuth?: boolean;
};

async function request<T>(path: string, opts: RequestOptions = {}): Promise<T> {
    const { method = "GET", body, headers = {}, signal, skipAuth = false } = opts;

    const url = `${getBaseUrl()}${path}`;
    const isFormData = typeof FormData !== "undefined" && body instanceof FormData;
    const finalHeaders: Record<string, string> = { ...headers };

    if (!isFormData)
        finalHeaders["Content-Type"] = "application/json";

    if (!skipAuth) {
        const token = getToken();
        if (token) {
            finalHeaders["Authorization"] = `Bearer ${token}`;
        }
    }

    let res: Response;
    try {
        res = await fetch(url, {
            method,
            headers: finalHeaders,
            body: body !== undefined
                ? isFormData
                    ? body as FormData
                    : JSON.stringify(body)
                : undefined,
            signal,
        });
    } catch {
        throw new ApiError(0, "Unable to reach the server. Please check your connection and try again.");
    }

    if (res.status === 401) {
        clearToken();
        clearStoredUser();
        throw new ApiError(401, "Your session has expired. Please sign in again.");
    }

    if (res.status === 403) {
        throw new ApiError(403, "You do not have permission to perform this action.");
    }

    if (res.status === 404) {
        throw new ApiError(404, "The requested resource was not found.");
    }

    if (!res.ok) {
        let errorBody: unknown;
        let message = `Request failed with status ${res.status}`;
        try {
            errorBody = await res.json();
            if (errorBody && typeof errorBody === "object" && "message" in errorBody) {
                message = String((errorBody as Record<string, unknown>).message);
            } else if (typeof errorBody === "string") {
                message = errorBody;
            }
        } catch {
            // response has no JSON body
        }
        throw new ApiError(res.status, message, errorBody);
    }

    if (res.status === 204) {
        return undefined as T;
    }

    const text = await res.text();
    if (!text) {
        return undefined as T;
    }
    try {
        return JSON.parse(text) as T;
    } catch {
        return text as unknown as T;
    }
}

export const api = {
    get: <T>(path: string, opts?: Omit<RequestOptions, "method" | "body">) =>
        request<T>(path, { ...opts, method: "GET" }),

    post: <T>(path: string, body?: unknown, opts?: Omit<RequestOptions, "method" | "body">) =>
        request<T>(path, { ...opts, method: "POST", body }),

    put: <T>(path: string, body?: unknown, opts?: Omit<RequestOptions, "method" | "body">) =>
        request<T>(path, { ...opts, method: "PUT", body }),

    patch: <T>(path: string, body?: unknown, opts?: Omit<RequestOptions, "method" | "body">) =>
        request<T>(path, { ...opts, method: "PATCH", body }),

    delete: <T>(path: string, opts?: Omit<RequestOptions, "method" | "body">) =>
        request<T>(path, { ...opts, method: "DELETE" }),
};

// Auth DTOs

export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthResponse {
    token: string;
    email: string;
    firstName: string;
    lastName: string;
    avatarFileId?: string;
    isActive: boolean;
    role: number | string;
}

function decodeToken(token: string): Record<string, unknown> {
    try {
        // Non TS source type enforcement
        if (typeof token !== "string") return {};

        const parts = token.split(".");
        if (parts.length !== 3) return {};

        const encoded = parts[1];
        if (!encoded) return {};

        // Base64URL -> Base64
        const base64 = encoded
            .replace(/-/g, "+")
            .replace(/_/g, "/");

        // Pad to a multiple of 4 (pad count is 0..3)
        const pad = (4 - (base64.length % 4)) % 4;
        const padded = base64 + "=".repeat(pad);

        // Decode to bytes, then UTF-8 decode (handles non-ASCII correctly)
        const binary = atob(padded);
        const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0));
        const json = new TextDecoder("utf-8", { fatal: true }).decode(bytes);

        const parsed: unknown = JSON.parse(json);
        if (parsed === null || typeof parsed !== "object" || Array.isArray(parsed)) {
            return {};
        }

        return parsed as Record<string, unknown>;
    } catch {
        return {};
    }
}

export function mapAuthUser(response: AuthResponse): User {
    const claims = decodeToken(response.token);

    const rawRole = response.role;
    const role: UserRole = typeof rawRole === "number"
        ? (["student", "instructor", "admin"][rawRole] as UserRole) ?? "student"
        : rawRole.toLowerCase() as UserRole;

    return {
        id: String(claims.sub ?? response.email),
        email: response.email,
        firstName: response.firstName,
        lastName: response.lastName,
        avatarFileId: response.avatarFileId,
        isActive: response.isActive,
        role,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
    };
}
