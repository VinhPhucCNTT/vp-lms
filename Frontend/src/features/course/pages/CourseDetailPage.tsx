import { useQuery } from "@tanstack/react-query";
import { Collapse, List, Space, Tag, Typography } from "antd";
import { Link, useParams } from "react-router-dom";

import { getCourseContent } from "@/features/courses/api/courseApi";
import { ActivityType } from "@/shared/types/lms";

const { Title, Text } = Typography;

function activityLink(courseId: string, type: ActivityType, resourceId: string | null) {
  if (!resourceId) return null;
  switch (type) {
    case ActivityType.Lesson:
      return `/courses/${courseId}/lesson/${resourceId}`;
    case ActivityType.Assignment:
      return `/courses/${courseId}/assignment/${resourceId}`;
    case ActivityType.Assessment:
      return `/courses/${courseId}/assessment/${resourceId}`;
    default:
      return null;
  }
}

function typeLabel(t: ActivityType) {
  switch (t) {
    case ActivityType.Lesson:
      return "Lesson";
    case ActivityType.Assessment:
      return "Exam";
    case ActivityType.Assignment:
      return "Assignment";
    default:
      return "Activity";
  }
}

export default function CourseDetailPage() {
  const { courseId } = useParams<{ courseId: string }>();

  const { data, isLoading, error } = useQuery({
    queryKey: ["course", courseId, "content"],
    queryFn: () => getCourseContent(courseId!),
    enabled: !!courseId,
  });

  if (!courseId) return <Text type="danger">Missing course id.</Text>;
  if (isLoading) return <Text>Loading course…</Text>;
  if (error || !data)
    return <Text type="danger">Could not load this course.</Text>;

  return (
    <div>
      <Title level={2}>{data.title}</Title>
      <Text type="secondary">Modules and activities</Text>

      <Collapse
        style={{ marginTop: 16 }}
        items={data.modules.map((m) => ({
          key: m.id,
          label: (
            <Space>
              <strong>{m.title}</strong>
              <Tag>{m.activities?.length ?? 0} activities</Tag>
            </Space>
          ),
          children: (
            <List
              dataSource={m.activities ?? []}
              renderItem={(a) => {
                const href = activityLink(courseId, a.type, a.resourceId);
                return (
                  <List.Item>
                    <Space>
                      <Tag>{typeLabel(a.type)}</Tag>
                      {href ? (
                        <Link to={href}>{a.title}</Link>
                      ) : (
                        <Text type="secondary">{a.title} (not linked)</Text>
                      )}
                    </Space>
                  </List.Item>
                );
              }}
            />
          ),
        }))}
      />
    </div>
  );
}
