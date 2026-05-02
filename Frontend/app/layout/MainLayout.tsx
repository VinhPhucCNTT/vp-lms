import { AppShell, Burger, Group, Text } from "@mantine/core";
import { useState } from "react";
import { Outlet } from "react-router";

export default function MainLayout() {
    const [isOpen, setIsOpen] = useState(false);

    return (
        <AppShell
            header={{ height: 60 }}
            navbar={{ width: 300, breakpoint: 'sm', collapsed: { mobile: !isOpen } }}
            padding="md"
        >
            <AppShell.Header>
                <Group h="100%" px="md">
                    <Burger opened={isOpen} onClick={() => setIsOpen(!isOpen)} hiddenFrom="sm" size="sm" />
                    Header has a burger icon below sm breakpoint
                </Group>
            </AppShell.Header>
            <AppShell.Navbar p="md">
                Navbar is collapsed on mobile at sm breakpoint. At that point it is no longer offset by
                padding in the main element and it takes the full width of the screen when opened.
            </AppShell.Navbar>
            <AppShell.Main>
                <Outlet />
            </AppShell.Main>
        </AppShell>
    );
}
