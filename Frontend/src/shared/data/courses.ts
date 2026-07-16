import type { Course, Module, Lesson, Assignment, Assessment, CourseActivity, StudentActivity, ActivityStatus } from "@/types";

export const courses: Course[] = [
  { id: "cs-101", code: "CS 101", title: "Introduction to Algorithms", description: "A foundational course covering algorithm design, analysis, and problem-solving techniques.", instructorId: "ins-001", status: "published", enrolledCount: 156, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01", department: "Computer Science", credits: 3, level: "beginner", semester: "Spring 2026", tags: ["algorithms", "foundations"], featured: true },
  { id: "cs-201", code: "CS 201", title: "Operating Systems", description: "Study of operating system concepts including process management, memory management, file systems.", instructorId: "ins-002", status: "published", enrolledCount: 98, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01", department: "Computer Science", credits: 4, level: "intermediate", semester: "Spring 2026", tags: ["systems", "os"], featured: false },
  { id: "cs-301", code: "CS 301", title: "Database Systems", description: "Comprehensive study of database design, SQL, normalization, transaction processing.", instructorId: "ins-003", status: "published", enrolledCount: 87, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01", department: "Computer Science", credits: 3, level: "intermediate", semester: "Spring 2026", tags: ["databases", "sql"], featured: true },
  { id: "cs-401", code: "CS 401", title: "Web Development", description: "Full-stack web development covering modern frameworks, REST APIs, authentication.", instructorId: "ins-004", status: "published", enrolledCount: 134, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01", department: "Computer Science", credits: 3, level: "intermediate", semester: "Spring 2026", tags: ["web", "fullstack"], featured: true },
  { id: "cs-501", code: "CS 501", title: "Machine Learning", description: "Introduction to machine learning algorithms including supervised and unsupervised learning.", instructorId: "ins-005", status: "published", enrolledCount: 72, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01", department: "Data Science", credits: 4, level: "advanced", semester: "Spring 2026", tags: ["ml", "ai"], featured: true },
  { id: "cs-601", code: "CS 601", title: "Software Engineering", description: "Advanced software engineering practices including agile methodologies, design patterns.", instructorId: "ins-006", status: "published", enrolledCount: 89, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01", department: "Software Engineering", credits: 3, level: "advanced", semester: "Spring 2026", tags: ["engineering", "agile"], featured: false },
  { id: "cs-102", code: "CS 102", title: "Data Structures", description: "Comprehensive study of fundamental data structures including arrays, linked lists, trees.", instructorId: "ins-001", status: "published", enrolledCount: 142, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01", department: "Computer Science", credits: 3, level: "beginner", semester: "Spring 2026", tags: ["data-structures", "foundations"], featured: false },
  { id: "cs-202", code: "CS 202", title: "Computer Networks", description: "Study of computer network architectures, protocols, and security.", instructorId: "ins-002", status: "draft", enrolledCount: 0, startDate: "2026-06-01", endDate: "2026-08-15", createdAt: "2025-12-15", department: "Computer Science", credits: 3, level: "intermediate", semester: "Summer 2026", tags: ["networks", "security"], featured: false },
  { id: "math-101", code: "MATH 101", title: "Calculus I", description: "Introduction to differential and integral calculus, limits, continuity, and applications.", instructorId: "ins-001", status: "published", enrolledCount: 210, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-10-15", department: "Mathematics", credits: 4, level: "beginner", semester: "Spring 2026", tags: ["calculus", "foundations"], featured: true },
  { id: "math-201", code: "MATH 201", title: "Linear Algebra", description: "Vector spaces, matrices, eigenvalues, and applications to computer science.", instructorId: "ins-002", status: "published", enrolledCount: 95, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-10-20", department: "Mathematics", credits: 3, level: "intermediate", semester: "Spring 2026", tags: ["linear-algebra", "matrices"], featured: false },
  { id: "eng-101", code: "ENG 101", title: "Digital Logic Design", description: "Fundamentals of digital systems, Boolean algebra, combinational and sequential circuits.", instructorId: "ins-003", status: "published", enrolledCount: 78, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-05", department: "Engineering", credits: 4, level: "beginner", semester: "Spring 2026", tags: ["digital", "hardware"], featured: false },
  { id: "eng-201", code: "ENG 201", title: "Embedded Systems", description: "Microcontroller programming, real-time systems, and hardware-software interfacing.", instructorId: "ins-004", status: "published", enrolledCount: 45, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-10", department: "Engineering", credits: 4, level: "advanced", semester: "Spring 2026", tags: ["embedded", "hardware"], featured: false },
];

export const modules: Module[] = [
  { id: "mod-001", courseId: "cs-101", title: "Introduction to Algorithm Analysis", order: 1, activityIds: ["act-001", "act-002", "act-003", "act-004"] },
  { id: "mod-002", courseId: "cs-101", title: "Sorting Algorithms", order: 2, activityIds: ["act-005", "act-006", "act-007"] },
  { id: "mod-003", courseId: "cs-101", title: "Searching and Hashing", order: 3, activityIds: ["act-008"] },
];

export const lessons: Lesson[] = [
  { id: "les-001", moduleId: "mod-001", title: "What is an Algorithm?", type: "video", content: "<h2>Introduction</h2><p>An algorithm is a step-by-step procedure for solving a problem in a finite amount of time.</p><h3>Learning Objectives</h3><ul><li>Define what an algorithm is</li><li>Understand the difference between algorithms and programs</li></ul>", duration: 45, order: 1 },
  { id: "les-002", moduleId: "mod-001", title: "Time Complexity Analysis", type: "reading", content: "<h2>Big O Notation</h2><p>Big O notation is used to describe the limiting behavior of a function.</p><h3>Common Time Complexities</h3><ul><li>O(1) - Constant time</li><li>O(log n) - Logarithmic time</li><li>O(n) - Linear time</li></ul>", duration: 30, order: 2 },
];

export const assignments: Assignment[] = [
  { id: "asg-001", moduleId: "mod-001", title: "Algorithm Analysis Practice", description: "Analyze the time complexity of 5 different algorithms. Provide Big O, Big Omega, and Big Theta for each.", dueDate: "2026-01-20", maxScore: 100, weight: 15 },
  { id: "asg-002", moduleId: "mod-002", title: "Implement Sorting Algorithms", description: "Implement QuickSort, MergeSort, and HeapSort in Python. Compare their performance.", dueDate: "2026-02-01", maxScore: 150, weight: 20 },
];

export const assessments: Assessment[] = [
  { id: "asm-001", moduleId: "mod-001", title: "Algorithm Analysis Quiz", description: "Test your understanding of basic algorithm analysis concepts.", duration: 30, passingScore: 70, maxAttempts: 3, dueDate: "2026-01-18" },
  { id: "asm-002", moduleId: "mod-002", title: "Sorting Algorithms Midterm", description: "Comprehensive test on sorting algorithms.", duration: 60, passingScore: 60, maxAttempts: 1, dueDate: "2026-02-15" },
];

export const courseActivities: CourseActivity[] = [
  { id: "act-001", moduleId: "mod-001", courseId: "cs-101", type: "lesson", title: "What is an Algorithm?", order: 1, refId: "les-001" },
  { id: "act-002", moduleId: "mod-001", courseId: "cs-101", type: "lesson", title: "Time Complexity Analysis", order: 2, refId: "les-002" },
  { id: "act-003", moduleId: "mod-001", courseId: "cs-101", type: "assignment", title: "Algorithm Analysis Practice", order: 3, refId: "asg-001" },
  { id: "act-004", moduleId: "mod-001", courseId: "cs-101", type: "assessment", title: "Algorithm Analysis Quiz", order: 4, refId: "asm-001" },
  { id: "act-005", moduleId: "mod-002", courseId: "cs-101", type: "coding-problem", title: "Two Sum", order: 1, refId: "prob-001" },
  { id: "act-006", moduleId: "mod-002", courseId: "cs-101", type: "coding-problem", title: "Merge Sort Implementation", order: 2, refId: "prob-003" },
  { id: "act-007", moduleId: "mod-002", courseId: "cs-101", type: "assignment", title: "Implement Sorting Algorithms", order: 3, refId: "asg-002" },
  { id: "act-008", moduleId: "mod-003", courseId: "cs-101", type: "coding-problem", title: "Binary Search", order: 1, refId: "prob-002" },
];

export function calculateActivityStatus(dueDate: string, completed: boolean = false): ActivityStatus {
  if (completed) return "completed";
  const now = new Date();
  const due = new Date(dueDate);
  return due < now ? "overdue" : "pending";
}

export function getStudentActivities(enrolledCourseIds: string[]): StudentActivity[] {
  const activities: StudentActivity[] = [];

  enrolledCourseIds.forEach(courseId => {
    const course = courses.find(c => c.id === courseId);
    if (!course) return;

    const courseActs = courseActivities.filter(a => a.courseId === courseId);

    courseActs.forEach(activity => {
      let dueDate = "";
      // Deterministic pseudo-random based on activity id so status is stable across renders
      const seed = activity.id.split("").reduce((acc, c) => acc + c.charCodeAt(0), 0);
      const completed = (seed % 10) > 3;

      if (activity.type === "assignment") {
        const assignment = assignments.find(a => a.id === activity.refId);
        if (assignment) dueDate = assignment.dueDate;
      } else if (activity.type === "assessment") {
        const assessment = assessments.find(a => a.id === activity.refId);
        if (assessment) dueDate = assessment.dueDate;
      } else if (activity.type === "coding-problem") {
        dueDate = "2026-07-15";
      } else if (activity.type === "lesson") {
        dueDate = "2026-07-20";
      }

      activities.push({
        id: activity.id,
        title: activity.title,
        type: activity.type,
        course: { id: course.id, code: course.code, title: course.title },
        dueDate,
        status: calculateActivityStatus(dueDate, completed),
        refId: activity.refId,
        moduleId: activity.moduleId,
      });
    });
  });

  return activities.sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());
}

export const departments = ["Computer Science", "Mathematics", "Engineering", "Data Science", "Software Engineering"] as const;
export const semesters = ["Spring 2026", "Summer 2026", "Fall 2026"] as const;
export const levels = ["beginner", "intermediate", "advanced"] as const;
