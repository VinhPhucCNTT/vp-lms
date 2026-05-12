import { Divider, Stack, Paper, Text, SimpleGrid, Title } from "@mantine/core";
import {
  IconUsersGroup,
  IconLayoutGridAdd,
  IconClock,
  IconCheck,
  IconPencilExclamation,
  IconLayoutGrid,
} from "@tabler/icons-react";

import DBStatTile from "../components/DBStatTile.tsx";
import DBActivityTile from "../components/DBActivityTile.tsx";
import DBCourseRow from "../components/DBCourseRow.tsx";
import classes from "./DashboardIndex.module.css";

type Course = {
  id: Number;
  name: string;
  desc: string;
};

type Activity = {
  id: Number;
  courseId: Number;
  name: string;
  desc: string;
  type: string;
};

const mockdata = {
  coursesEnrolled: 4,
  coursesCreated: 1,
  coursesInProgress: 2,
  coursesCompleted: 2,
  totalStudents: 100,
  assignmentsWaiting: 10,
  recentCourses: [
    {
      id: 1,
      name: "CS101",
      desc: "Programming fundamentals",
    },
    {
      id: 2,
      name: "CS102",
      desc: "Data structures & Algorithms",
    },
    {
      id: 3,
      name: "CS201",
      desc: "Information systems",
    },
    {
      id: 4,
      name: "CS204",
      desc: "Web programming",
    },
    {
      id: 5,
      name: "CS402",
      desc: "Pure functional programming in Haskell",
    },
  ] satisfies Course[],
  upcomingActivities: [
    {
      id: 1,
      courseId: 1,
      name: "Programming problem #1",
      desc: "",
      type: "assessment",
    },
    {
      id: 1,
      courseId: 4,
      name: "Final assignment submission",
      desc: "",
      type: "assignment",
    },
  ] satisfies Activity[],
};

export default function DashboardIndex() {
  return (
    <div>
      <Title order={1} mb="md">
        Welcome back!
      </Title>
      <Title order={3} mb="md" className={classes.title}>
        STATISTICS:
      </Title>
      <SimpleGrid mb="md" cols={{ xs: 1, md: 3, xl: 6 }}>
        <DBStatTile title="Courses Enrolled" amount={mockdata.coursesEnrolled}>
          <IconLayoutGrid />
        </DBStatTile>
        <DBStatTile title="Courses Created" amount={mockdata.coursesCreated}>
          <IconLayoutGridAdd />
        </DBStatTile>
        <DBStatTile
          title="Courses In Progress"
          amount={mockdata.coursesInProgress}
        >
          <IconClock />
        </DBStatTile>
        <DBStatTile title="Completed" amount={mockdata.coursesCompleted}>
          <IconCheck />
        </DBStatTile>
        <DBStatTile title="Total Students" amount={mockdata.totalStudents}>
          <IconUsersGroup />
        </DBStatTile>
        <DBStatTile
          title="Assignments Waiting"
          amount={mockdata.assignmentsWaiting}
        >
          <IconPencilExclamation />
        </DBStatTile>
      </SimpleGrid>
      <Divider mb="md" />

      <SimpleGrid cols={{ sm: 1, md: 2 }}>
        <div className={classes.gridContainer}>
          <Title order={3} mb="md" className={`${classes.title} ${classes.hoverable}`}>
            RECENT:
          </Title>
          <Stack gap="lg">
            {mockdata.recentCourses.map((course) => (
              <DBCourseRow
                info={course}
                lastAccessed={new Date("2026-05-12T15:07:00")}
              />
            ))}
          </Stack>
        </div>

        <div className={classes.gridContainer}>
          <Title order={3} className={`${classes.title} ${classes.hoverable}`}>
            UPCOMING:
          </Title>
          <Text fs="italic" size="mg" c="dimmed" mb="md">
            You have 10 activities in total.
          </Text>
          <SimpleGrid cols={{ sm: 1, md: 3 }}>
            {mockdata.upcomingActivities.map((activity) => (
              <DBActivityTile
                info={activity}
                courseName={mockdata.recentCourses[activity.courseId].name}
                expiringInDays={2}
              />
            ))}
          </SimpleGrid>
        </div>
      </SimpleGrid>
    </div>
  );
}
