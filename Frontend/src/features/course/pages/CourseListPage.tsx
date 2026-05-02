import { Typography } from "antd";

import CourseTable from "../components/CourseTable";

const { Title } = Typography;

export default function CourseListPage() {
  return (
    <div>
      <Title level={2}>Courses</Title>
      <CourseTable />
    </div>
  );
}
