export interface IUser {
    email: string;
    roles: string[];
    userId: string;
}

export interface IAuthContext {
    user: IUser | null;
    token: string | null;
    isAuthenticated: boolean;

    login: (token: string) => void;
    logout: () => void;
}
