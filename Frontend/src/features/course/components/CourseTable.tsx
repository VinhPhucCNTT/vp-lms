import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Button,
  Modal,
  Space,
  Table,
  Tag,
  Form,
  Input,
  Switch,
  Typography,
  message,
} from "antd";
import { Link } from "react-router-dom";

import { useAuthContext } from "@/features/auth/context/useAuthContext";
import { enrollInCourse } from "@/features/enrollments/api/enrollmentApi";
import {
  createCourse,
  deleteCourse,
  getCourses,
  updateCourse,
} from "@/features/courses/api/courseApi";
import { getPrimaryRole } from "@/shared/helpers/role";
import type { CreateCoursePayload, ViewCourse } from "@/shared/types/lms";

const { Text } = Typography;

export default function CourseTable() {
  const { user } = useAuthContext();
  const role = getPrimaryRole(user?.roles);
  const isInstructor = role === "Instructor";
  const isStudent = role === "Student";
  const queryClient = useQueryClient();
  const [messageApi, ctx] = message.useMessage();

  const { data: courses = [], isLoading } = useQuery({
    queryKey: ["courses"],
    queryFn: getCourses,
  });

  const myCourses =
    isInstructor && user?.userId
      ? courses.filter((c) => c.instructorId === user.userId)
      : courses;

  const enrollMutation = useMutation({
    mutationFn: enrollInCourse,
    onSuccess: () => {
      messageApi.success("Enrolled successfully.");
      void queryClient.invalidateQueries({ queryKey: ["enrollments", "my"] });
    },
    onError: () => {
      messageApi.error("Could not enroll. You may already be enrolled.");
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteCourse,
    onSuccess: () => {
      messageApi.success("Course deleted.");
      void queryClient.invalidateQueries({ queryKey: ["courses"] });
    },
    onError: () => messageApi.error("Failed to delete course."),
  });

  const [createOpen, setCreateOpen] = useState(false);
  const [editCourse, setEditCourse] = useState<ViewCourse | null>(null);
  const [form] = Form.useForm<CreateCoursePayload>();

  useEffect(() => {
    if (editCourse) {
      form.setFieldsValue({
        title: editCourse.title,
        description: editCourse.description ?? "",
        thumpnailUrl: editCourse.thumbnailUrl ?? "",
        isPublished: editCourse.isPublished,
      });
    } else if (createOpen) {
      form.resetFields();
      form.setFieldsValue({
        title: "",
        description: "",
        thumpnailUrl: "",
        isPublished: false,
      });
    }
  }, [createOpen, editCourse, form]);

  const saveMutation = useMutation({
    mutationFn: async (values: CreateCoursePayload) => {
      if (editCourse) {
        await updateCourse(editCourse.id, values);
      } else {
        await createCourse(values);
      }
    },
    onSuccess: () => {
      messageApi.success(editCourse ? "Course updated." : "Course created.");
      setCreateOpen(false);
      setEditCourse(null);
      void queryClient.invalidateQueries({ queryKey: ["courses"] });
    },
    onError: () => messageApi.error("Save failed."),
  });

  return (
    <div>
      {ctx}
      <Space
        style={{
          marginBottom: 16,
          width: "100%",
          justifyContent: "space-between",
        }}
      >
        <Text type="secondary">
          {isInstructor
            ? "Courses you teach"
            : "Browse published courses and enroll."}
        </Text>
        {isInstructor && (
          <Button type="primary" onClick={() => setCreateOpen(true)}>
            New course
          </Button>
        )}
      </Space>

      <Table<ViewCourse>
        rowKey="id"
        loading={isLoading}
        dataSource={isInstructor ? myCourses : courses}
        pagination={{ pageSize: 10 }}
        columns={[
          {
            title: "Title",
            dataIndex: "title",
            render: (t: string, record) => (
              <Link to={`/courses/${record.id}`}>{t}</Link>
            ),
          },
          {
            title: "Instructor",
            dataIndex: "instructorFullName",
            width: 180,
          },
          {
            title: "Modules",
            dataIndex: "moduleCount",
            width: 90,
          },
          {
            title: "Enrollments",
            dataIndex: "enrollmentCount",
            width: 110,
          },
          {
            title: "Status",
            dataIndex: "isPublished",
            width: 100,
            render: (v: boolean) =>
              v ? <Tag color="green">Published</Tag> : <Tag>Draft</Tag>,
          },
          {
            title: "",
            key: "actions",
            width: 220,
            render: (_, record) => (
              <Space>
                <Link to={`/courses/${record.id}`}>Open</Link>
                {isStudent && (
                  <Button
                    size="small"
                    type="primary"
                    loading={enrollMutation.isPending}
                    onClick={() => enrollMutation.mutate(record.id)}
                  >
                    Enroll
                  </Button>
                )}
                {isInstructor && record.instructorId === user?.userId && (
                  <>
                    <Button
                      size="small"
                      onClick={() => {
                        setEditCourse(record);
                        setCreateOpen(true);
                      }}
                    >
                      Edit
                    </Button>
                    <Link to={`/courses/${record.id}/build`}>Build</Link>
                    <Button
                      size="small"
                      danger
                      loading={deleteMutation.isPending}
                      onClick={() => {
                        Modal.confirm({
                          title: "Delete this course?",
                          onOk: () => deleteMutation.mutate(record.id),
                        });
                      }}
                    >
                      Delete
                    </Button>
                  </>
                )}
              </Space>
            ),
          },
        ]}
      />

      <Modal
        title={editCourse ? "Edit course" : "Create course"}
        open={createOpen}
        onCancel={() => {
          setCreateOpen(false);
          setEditCourse(null);
        }}
        footer={null}
        destroyOnClose
      >
        <Form<CreateCoursePayload>
          form={form}
          layout="vertical"
          onFinish={(v) => saveMutation.mutate(v)}
        >
          <Form.Item name="title" label="Title" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} />
          </Form.Item>
          <Form.Item name="thumpnailUrl" label="Thumbnail URL">
            <Input />
          </Form.Item>
          <Form.Item
            name="isPublished"
            label="Published"
            valuePropName="checked"
          >
            <Switch />
          </Form.Item>
          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              loading={saveMutation.isPending}
            >
              Save
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
