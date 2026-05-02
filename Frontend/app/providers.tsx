import type { ReactNode } from "react";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MantineProvider } from "@mantine/core";
import { AuthProvider } from "../src/features/auth/contexts/AuthProvider";

const queryClient = new QueryClient();

interface ProviderProps {
    children: ReactNode;
}

export default function Providers({ children }: ProviderProps) {
    return (
        <QueryClientProvider client={queryClient}>
            <MantineProvider>
                <AuthProvider>
                    {children}
                </AuthProvider>
            </MantineProvider>
        </QueryClientProvider>
    );
}
