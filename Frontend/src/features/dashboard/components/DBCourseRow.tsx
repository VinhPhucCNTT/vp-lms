import { Title, Text, Paper } from "@mantine/core";

import classes from "./DBCourseRow.module.css";

type Course = {
  id: Number;
  name: string;
  desc: string;
};

interface DBCourseRowProps {
  info: Course;
  lastAccessed: Date;
}

export default function DBCourseRow({ info, lastAccessed }: DBCourseRowProps) {
  return (
    <Paper className={classes.row}>
      <Title order={4} mb="xs" >
        {info.name}
        <Text component="span" c="dimmed"> #{info.id.toString()}</Text>
      </Title>
      {info.desc == "" ? (
        <Text c="dimmed" fs="italic" className={classes.desc}>
          No description provided.
        </Text>
      ) : (
        <Text className={classes.desc} truncate="end">
          {info.desc}
        </Text>
      )}
      <Text c="dimmed" fs="italic" size="sm">Last accessed on: {lastAccessed.toString()}</Text>
    </Paper>
  );
}
