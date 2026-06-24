import type { Course, Module, Lesson, Assignment, Assessment } from "@/types";

export const courses: Course[] = [
  { id: "cs-101", code: "CS 101", title: "Introduction to Algorithms", description: "A foundational course covering algorithm design, analysis, and problem-solving techniques.", instructorId: "ins-001", status: "published", enrolledCount: 156, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01" },
  { id: "cs-201", code: "CS 201", title: "Operating Systems", description: "Study of operating system concepts including process management, memory management, file systems.", instructorId: "ins-002", status: "published", enrolledCount: 98, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01" },
  { id: "cs-301", code: "CS 301", title: "Database Systems", description: "Comprehensive study of database design, SQL, normalization, transaction processing.", instructorId: "ins-003", status: "published", enrolledCount: 87, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01" },
  { id: "cs-401", code: "CS 401", title: "Web Development", description: "Full-stack web development covering modern frameworks, REST APIs, authentication.", instructorId: "ins-004", status: "published", enrolledCount: 134, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01" },
  { id: "cs-501", code: "CS 501", title: "Machine Learning", description: "Introduction to machine learning algorithms including supervised and unsupervised learning.", instructorId: "ins-005", status: "published", enrolledCount: 72, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01" },
  { id: "cs-601", code: "CS 601", title: "Software Engineering", description: "Advanced software engineering practices including agile methodologies, design patterns.", instructorId: "ins-006", status: "published", enrolledCount: 89, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01" },
  { id: "cs-102", code: "CS 102", title: "Data Structures", description: "Comprehensive study of fundamental data structures including arrays, linked lists, trees.", instructorId: "ins-001", status: "published", enrolledCount: 142, startDate: "2026-01-06", endDate: "2026-05-15", createdAt: "2025-11-01" },
  { id: "cs-202", code: "CS 202", title: "Computer Networks", description: "Study of computer network architectures, protocols, and security.", instructorId: "ins-002", status: "draft", enrolledCount: 0, startDate: "2026-06-01", endDate: "2026-08-15", createdAt: "2025-12-15" },
];

export const modules: Module[] = [
  { id: "mod-001", courseId: "cs-101", title: "Introduction to Algorithm Analysis", order: 1 },
  { id: "mod-002", courseId: "cs-101", title: "Sorting Algorithms", order: 2 },
  { id: "mod-003", courseId: "cs-101", title: "Searching and Hashing", order: 3 },
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
