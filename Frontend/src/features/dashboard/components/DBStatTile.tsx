import { Paper, Text } from "@mantine/core"

import classes from "./DBStatTile.module.css";
import type { ReactNode } from "react";

interface DBStatTileProps {
    title: string,
    amount: Number,
    children?: ReactNode,
    color?: string,
}

export default function DBStatTile({ title, amount, children, color }: DBStatTileProps) {
    return <Paper className={classes.card} style={{'--dashboard-icon-bg': color}}>
        {children}
        <Text className={classes.amount}>{amount.toString()}</Text>
        <Text className={classes.title}>{title}</Text>
    </Paper>;
}
