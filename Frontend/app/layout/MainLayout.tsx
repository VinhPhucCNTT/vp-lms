import { AppShell, Burger, Container, Divider, Drawer, Group, ScrollArea } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useState } from "react";
import { Outlet } from "react-router";

import classes from "./MainLayout.module.css";

const links = [
    { link: '/dashboard', label: 'Dashboard' },
    { link: '/manage', label: 'Manage' },
    { link: '/explore', label: 'Explore' },
    { link: '/settings', label: 'Settings' },
];

export default function MainLayout() {
    const [opened, { toggle, close }] = useDisclosure(false);
    const [active, setActive] = useState(links[0].link);

    const items = links.map((link) => (
        <a
            key={link.label}
            href={link.link}
            className={classes.link}
            data-active={active === link.link || undefined}
            onClick={(event) => {
                event.preventDefault();
                setActive(link.link);
            }}
        >
            {link.label}
        </a>
    ));

    return (
        <AppShell
            header={{ height: 60 }}
            padding="md"
        >
            <AppShell.Header>
                <Container size="xl" className={classes.inner}>
                    <div className={classes.logo}>VP-LMS</div>
                    <Group gap={5} visibleFrom="xs">
                        {items}
                    </Group>

                    <Burger
                        opened={opened}
                        onClick={toggle}
                        hiddenFrom="xs"
                        size="sm"
                        aria-label="Toggle navigation"
                    />
                </Container>

                <Drawer
                    opened={opened}
                    onClose={close}
                    size="100%"
                    padding="md"
                    title="Navigation"
                    hiddenFrom="xs"
                    zIndex={1000000}
                >
                    <ScrollArea h="calc(100vh - 80px" mx="-md">
                        <Divider my="sm" />
                        {items}
                    </ScrollArea>
                </Drawer>
            </AppShell.Header>
            <AppShell.Main>
                <Outlet />
            </AppShell.Main>
        </AppShell>
    );
}
