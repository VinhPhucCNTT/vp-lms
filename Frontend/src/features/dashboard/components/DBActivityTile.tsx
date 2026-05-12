import {Paper, Text} from "@mantine/core";

import classes from "./DBActivityTile.module.css";
import { IconFileText, IconPencil, IconPrompt, IconQuestionMark } from "@tabler/icons-react";

type Activity = {
    id: Number,
    courseId: Number,
    name: string,
    desc: string,
    type: string
}

interface DBActivityTileProps {
    info: Activity,
    courseName: string,
    expiringInDays: Number
}

export default function DBActivityTile({ info, courseName, expiringInDays }: DBActivityTileProps) {
    return <div>
        <Paper className={classes.tile} style={{'--db-bg-icon': 'var(--svg-icon-post)'}}>
            {
                info.type === "lesson" ? <IconFileText className={classes.logo} /> :
                info.type === "assessment" ? <IconQuestionMark className={classes.logo} /> :
                info.type === "assignment" ? <IconPencil className={classes.logo} /> :
                info.type === "judge" ? <IconPrompt className={classes.logo} /> :
                <Text c="red" fw="bold" size="xl">ERROR: UNDEFINED TYPE</Text>
            }
            <Text fw="bold" className={classes.courseName}>{courseName + " / "}
                <Text component="span" className={classes.name}>{info.name}</Text>
                <Text c="dimmed" component="span" className={classes.id}>{" #" + info.id.toString()}</Text>
            </Text>
            {info.desc == ""
                ? <Text c="dimmed" fs="italic" className={classes.desc}>No description provided.</Text>
                : <Text className={classes.desc} truncate="end">{info.desc}</Text>
            }
            <Text>Expiring in {" "}
                <Text component="span" c="red" fw="bold">{expiringInDays.toString()}</Text>
                {" "} days.
            </Text>
        </Paper>
    </div>;
}
